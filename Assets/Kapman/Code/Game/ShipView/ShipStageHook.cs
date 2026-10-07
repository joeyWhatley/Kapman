// First run in the chart scene on 7 Oct 2026 (Design/ship_view/BUILD_LOG.md, part 3). What was not known before that
// run and is known now: UIDocument.sortingOrder does re-sort a document at run time, and a camera's texture comes out
// the right way up in an Image.
using Kapman.Game.Frame;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE 3D VIEW IN THE FRAME, WITH NO LINE CHANGED IN THE FRAME. It starts itself in the chart scene (a scene with a
    /// ChartWorld) when the kit is there (Resources/ShipStageKit), makes the stage, and hangs three things on the frame's
    /// own elements, found by their classes:
    ///   - in the ship window (FrameRoot's "frame-ship-view", where FrameShipView paints the drawn stand-in): a picture
    ///     that fills it and so covers the stand-in, showing the stage's texture; and at its top left the Swap button
    ///     (the user, 7 Oct 2026: "a simple button on the ... top left of the 3D sailing boat that switches place with the
    ///     map and miniaturises the map");
    ///   - behind every slip of the frame (first child of FrameRoot.Root): the same texture filling the screen, shown
    ///     when swapped.
    /// SWAPPED, the chart is not drawn into a texture: the map's own document is made small. Its root is given the ship
    /// window's rectangle and its sorting order is put just over the frame's (42.5: over the frame at 42, under the
    /// notices at 43 and the menu at 50), so the chart goes on working in the small window (it still pans, zooms and
    /// takes clicks) and a second Swap button on it brings it back. The small chart is drawn at a coarser scale
    /// (<see cref="miniChartScale"/> times the chart's own), so that it still shows the waters round her. No camera
    /// changes its target: the chart's camera (which draws nothing) always has the screen and the stage's camera always
    /// draws to its texture; only the texture's size and which picture shows it change.
    /// The swap stands down while the frame itself is hidden (the title, the chest open on the map).
    /// With the kit missing, no graphics device, or <see cref="Off"/> set, nothing is made and the drawn stand-in stays.
    /// THE CREW AND THE LOOKS: the hook seats the rowing crew (PackCrewRig) and puts the looks on the stage
    /// (PsxStageLook, starting in the kit's startLook). The key L goes round the looks ("a few different shaders ... psx
    /// style in a few different ways"), and a small word at the bottom left of the view says which one is on.
    /// </summary>
    [DefaultExecutionOrder(950)]                                        // after the frame (900), before the stage (960) and the map (1000)
    public sealed class ShipStageHook : MonoBehaviour
    {
        /// <summary>Tests and tools: no 3D view (the drawn stand-in stays). <see cref="TurnOff"/> also takes down one that is up.</summary>
        public static bool Off = StartsOff;
        /// <summary>What <see cref="Off"/> is when Play begins. False since 7 Oct 2026: the view is in the chart.</summary>
        public const bool StartsOff = false;
        public static ShipStageHook Current { get; private set; }
        /// <summary>The 3D view fills the screen and the chart is in the small window.</summary>
        public bool Swapped;

        // ZOOMED ALL THE WAY IN, THE CHART GIVES WAY TO THE SHIP ON HER WATER (the user, 7 Oct 2026, night: "if you zoom in
        // all the way you can see a boat sailing on a simple simulated water animation"). Scrolling on in past the
        // chart's closest brings her own view into the middle of the screen, seen from above as the chart was (the
        // stage's sea is the simulated water: its waves are worked out from the wind where she is); scrolling out
        // again over it gives the chart back, and the angle she was looked at from before.
        bool zoomedIn;
        int angleBeforeZoom = -1;
        float zoomedAt;
        MapView zoomMap;
        /// <summary>The ship's view is in the middle because the chart was zoomed in past its closest.</summary>
        public bool ZoomedIn => zoomedIn && Swapped;

        void ZoomIn()
        {
            if (Swapped || Showroom || TitleSea || stage == null)
                return;
            angleBeforeZoom = stage.Angle;
            stage.SetAngle(3);                                            // (from above: the chart's own way of looking)
            zoomedIn = true;
            zoomedAt = Time.unscaledTime;
            Swapped = true;
        }

        /// <summary>Each frame: the chart's asking is listened for; and while she is in the middle by the zoom, a scroll
        /// out over her (not over a window of the frame) gives the chart back.</summary>
        void ZoomStep(FrameRoot frame)
        {
            var map = MapView.Current;
            if (!ReferenceEquals(map, zoomMap))
            {
                if (zoomMap != null)
                    zoomMap.ZoomedPast -= ZoomIn;
                zoomMap = map;
                if (zoomMap != null)
                    zoomMap.ZoomedPast += ZoomIn;
            }
            if (!Swapped)
            {
                if (zoomedIn)
                {
                    // (put back by the Swap button, or by this: her angle as it was)
                    zoomedIn = false;
                    if (stage != null && angleBeforeZoom >= 0)
                        stage.SetAngle(angleBeforeZoom);
                    angleBeforeZoom = -1;
                }
                return;
            }
            var mouse = Mouse.current;
            if (!zoomedIn || mouse == null || frame == null || frame.Root == null || frame.Root.panel == null || Time.unscaledTime - zoomedAt < 0.35f)
                return;
            if (mouse.scroll.ReadValue().y >= -0.01f)
                return;                                                   // (not a scroll out)
            Vector2 screen = mouse.position.ReadValue();
            Vector2 at = RuntimePanelUtils.ScreenToPanel(frame.Root.panel, new Vector2(screen.x, Screen.height - screen.y));
            var under = frame.Root.panel.Pick(at);
            if (under != null && under != frame.Root)
                return;                                                   // (over a window, a button, the small chart: theirs)
            Swapped = false;
        }
        /// <summary>Swapped, the small chart is drawn at this many times the chart's own metres to a pixel.</summary>
        public float miniChartScale = 2.5f;

        ShipStageKit kit;
        ShipStage stage;
        FrameRoot framed;
        VisualElement shipView, mapRoot;
        Image small, big;
        Button swapSmall, swapMini, lookButton;
        UIDocument mapDoc, frameDoc;
        MapView miniMap;
        float mapOrder, mapScale;
        bool applied;
        static bool warned;
        PsxStageLook looks;
        Label wordSmall, wordBig;

        /// <summary>The looks on the stage (null before the stage is made): Next() is what the key L does.</summary>
        public PsxStageLook Looks => looks;
        /// <summary>The stage this hook made (null before the frame is up).</summary>
        public ShipStage Stage => stage;
        /// <summary>The chart and the view have changed places now (false while the frame is hidden, whatever <see cref="Swapped"/> says).</summary>
        public bool Applied => applied;
        /// <summary>The two Swap buttons (a test clicks them as a player would): on the ship window, and on the small chart.</summary>
        public Button SwapButtonOnShip => swapSmall;
        public Button SwapButtonOnChart => swapSmall;                     // (one button now, in the top bar: it changes them and changes them back)
        /// <summary>The button that goes to the next look (as the key L does).</summary>
        public Button LookButton => lookButton;
        /// <summary>The picture in the ship window, and the one that fills the screen.</summary>
        public Image SmallPicture => small;
        public Image BigPicture => big;

        /// <summary>The showroom (the opening's turn of the boat on a pink void: ShipStage.Showroom): the stage is drawn
        /// for the whole screen, with nobody aboard, in the console's look; whoever asked shows <see cref="StagePicture"/>.
        /// The look and the crew come back when it is put out.</summary>
        public bool Showroom;
        /// <summary>The title's sea (ShipStage.TitleSea): a storm seen from above, drawn for the whole screen in a look of
        /// purple tones. The showroom comes first where both are asked.</summary>
        public bool TitleSea;
        public string TitleSeaLook = "console";
        bool titleSeaOn;
        /// <summary>The lines the showroom is drawn at (0: the whole screen's): a small picture wants few.</summary>
        public int ShowroomLines;
        bool showroomOn;
        int lookBefore;
        /// <summary>The stage's picture as it is drawn now (null until there is one).</summary>
        public Texture StagePicture => stage != null && stage.View != null ? stage.View.Picture : null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = null;
            // (KAPMAN_SHIP_VIEW_OFF=1 in the environment: a run of the game or of its tests without the 3D view)
            Off = StartsOff || System.Environment.GetEnvironmentVariable("KAPMAN_SHIP_VIEW_OFF") == "1";
            warned = false;
            SceneManager.sceneLoaded -= Loaded;
            SceneManager.sceneLoaded += Loaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void First() => TryStart();

        static void Loaded(Scene scene, LoadSceneMode mode) => TryStart();

        static void TryStart()
        {
            if (Off || Current != null || ChartWorld.Current == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                return;
            var kit = ShipStageKit.Load();
            if (kit == null)
            {
                if (!warned)
                    Debug.Log("Kapman ship view: no Resources/" + ShipStageKit.ResourceName + " (Kapman > Ship view > Make the kit): the ship window keeps its drawn stand-in.");
                warned = true;
                return;
            }
            var go = new GameObject("Ship view (3D)");
            Current = go.AddComponent<ShipStageHook>();
            Current.kit = kit;
        }

        /// <summary>Puts the view up now, in a chart scene that is already open (a setting, a test that measures).</summary>
        public static void TurnOn()
        {
            Off = false;
            TryStart();
        }

        /// <summary>Takes the view down now, stage and all: the frame's drawn stand-in shows again.</summary>
        public static void TurnOff()
        {
            Off = true;
            if (Current == null)
                return;
            var was = Current;
            was.TakeDown(true);
            Destroy(was.gameObject);
        }

        void OnDestroy() => TakeDown(false);

        void TakeDown(bool atOnce)
        {
            if (applied)
                Apply(false);
            Detach();
            if (stage != null)
            {
                // (switched off by hand, at once: the stage and its water are gone before the next frame is drawn or measured;
                // with the scene going down, Unity takes it in its own time)
                if (atOnce)
                    DestroyImmediate(stage.gameObject);
                else
                    Destroy(stage.gameObject);
            }
            stage = null;
            looks = null;
            if (Current == this)
                Current = null;
        }

        void Update()
        {
            if (Current != this)
                return;
            var frame = FrameRoot.Current;
            if (frame == null || frame.Root == null)
            {
                if (stage != null)
                    stage.Running = false;
                return;
            }
            if (frame != framed || shipView == null || shipView.panel == null)
                Attach(frame);
            if (shipView == null)
                return;
            if (stage == null)
                stage = ShipStage.Build(kit, new ChartStageSource());
            if (stage.Crew == null && !Showroom && !TitleSea)
                stage.Crew = new PackCrewRig(kit.men);
            if (looks == null)
            {
                // (the look last chosen with L is kept between runs, in the settings; a test starts from the kit's own)
                string start = kit.startLook;
                if (!FrameRoot.FreshLayout)
                    start = PlayerPrefs.GetString(LookPref, start);
                looks = new PsxStageLook(start);
                looks.Changed += Word;
                looks.Changed += Keep;
                stage.Look = looks;
                Word(looks.Style);
            }
            bool shown = frame.Root.resolvedStyle.display != DisplayStyle.None;
            // the key L: the next look
            var kb = Keyboard.current;
            if (kb != null && shown && !UiTyping.Active && !UiFocus.Modal && !kb.ctrlKey.isPressed && !kb.altKey.isPressed && kb.kKey.wasPressedThisFrame && ShipStage.Current != null)
                ShipStage.Current.SetAngle(ShipStage.Current.Angle + 1);        // [K]: the next camera angle
            if (kb != null && shown && !UiTyping.Active && !UiFocus.Modal && !kb.ctrlKey.isPressed && !kb.altKey.isPressed && kb.lKey.wasPressedThisFrame)
                looks.Next();
            ZoomStep(frame);
            var map = MapView.Current;
            bool want = Swapped && shown && map != null && !map.ChestOpen;
            if (want != applied)
                Apply(want);
            if (Showroom != showroomOn || TitleSea != titleSeaOn)
            {
                bool was = showroomOn || titleSeaOn, now = Showroom || TitleSea;
                if (!was && now)
                    lookBefore = looks.style;                             // (the player's own look, to come back to)
                showroomOn = Showroom;
                titleSeaOn = TitleSea;
                if (now)
                {
                    looks.SetStyle(Showroom ? "console" : TitleSeaLook);
                    stage.Crew = null;                                    // (nobody aboard)
                }
                else
                    looks.SetStyle(lookBefore);
                stage.Showroom = Showroom;
                stage.TitleSea = TitleSea && !Showroom;
            }
            stage.Running = shown || Showroom || TitleSea;
            stage.Big = applied || Showroom || TitleSea;
            Layout(frame);
        }

        /// <summary>Hangs the pictures and the button on the frame as it is built now (it builds itself anew when enabled).</summary>
        void Attach(FrameRoot frame)
        {
            if (applied)
                Apply(false);
            Detach();
            framed = frame;
            frameDoc = frame.GetComponent<UIDocument>();
            shipView = frame.Root.Q(null, "frame-ship-view");
            if (shipView == null)
                return;
            small = Picture();
            shipView.Add(small);
            // Swap and Look stand in the top bar with the other buttons that change what is looked at (the user's ruling of
            // 7 Oct 2026, late: "a good addition, just not well placed"); nothing is laid over the pictures.
            swapSmall = Kapman.Game.Frame.FrameUi.Btn("Swap", () => Swapped = !Swapped, "The ship in the middle of the screen and the chart in her window, and back.");
            swapSmall.focusable = false;
            lookButton = Kapman.Game.Frame.FrameUi.Btn("Look", () => { if (looks != null) looks.Next(); }, "The ship's picture in another style  [L]");
            lookButton.focusable = false;
            var bar = frame.ViewButtons ?? frame.Root;
            bar.Add(swapSmall);
            bar.Add(lookButton);
            big = Picture();
            big.style.display = DisplayStyle.None;
            frame.Root.Insert(0, big);
            // the word that says which look is on: on each picture, so it goes with whichever is shown
            wordSmall = wordBig = null;                                  // (the look is named on its button, in the bar)
            if (looks != null)
                Word(looks.Style);
            if (stage != null && stage.View != null)
                small.image = big.image = stage.View.Picture;
            applied = false;
        }

        /// <summary>A small quiet word at the bottom left of a picture (the frame's paper on its plum).</summary>
        static Label LookWord(int size)
        {
            var l = new Label { pickingMode = PickingMode.Ignore };
            l.style.position = Position.Absolute;
            l.style.left = 6;
            l.style.bottom = 5;
            l.AddToClassList("frame-ship-word");                  // the frame's own small size, so it follows the screen (was a fixed size)
            l.style.paddingLeft = l.style.paddingRight = 6;
            l.style.paddingTop = l.style.paddingBottom = 1;
            l.style.color = (Color)new Color32(236, 235, 230, 255);
            l.style.backgroundColor = (Color)new Color32(34, 22, 40, 170);
            l.style.borderTopLeftRadius = l.style.borderTopRightRadius = l.style.borderBottomLeftRadius = l.style.borderBottomRightRadius = 6;
            return l;
        }

        const string LookPref = "kapman.shipview.look";

        void Keep(PsxStyle style)
        {
            // (the showroom's look and the title's are the show's, not the player's choice: not kept)
            if (style != null && !FrameRoot.FreshLayout && !Showroom && !TitleSea && !showroomOn && !titleSeaOn)
                PlayerPrefs.SetString(LookPref, style.id);
        }

        /// <summary>The word for a look: its name, and the key that changes it. (For the look "None" only the key is told.)</summary>
        void Word(PsxStyle style)
        {
            string says = style == null || style.lines <= 0 ? "L: look" : style.name + "  (L)";
            if (wordSmall != null)
                wordSmall.text = says;
            if (wordBig != null)
                wordBig.text = says;
            if (lookButton != null)
                lookButton.tooltip = "The ship's picture in another style  [L]. Now: " + (style == null || style.lines <= 0 ? "as she is" : style.name) + ".";
        }

        void Detach()
        {
            small?.RemoveFromHierarchy();
            big?.RemoveFromHierarchy();
            swapSmall?.RemoveFromHierarchy();
            lookButton?.RemoveFromHierarchy();
            lookButton = null;
            small = big = null;
            wordSmall = wordBig = null;
            swapSmall = null;
            shipView = null;
            framed = null;
            frameDoc = null;
        }

        static Image Picture()
        {
            var i = new Image { scaleMode = ScaleMode.ScaleAndCrop, pickingMode = PickingMode.Ignore };
            i.style.position = Position.Absolute;
            i.style.left = 0;
            i.style.top = 0;
            i.style.right = 0;
            i.style.bottom = 0;
            return i;
        }

        /// <summary>The button, dressed here and not in a style sheet (Frame.uss is the frame builder's; and one of the two
        /// sits on the map's document, which has none of the frame's styles).</summary>
        Button SwapButton(string tip)
        {
            var b = new Button(() => Swapped = !Swapped) { text = "Swap", tooltip = tip, focusable = false };
            b.style.position = Position.Absolute;
            b.style.left = 6;
            b.style.top = 6;
            b.style.paddingLeft = b.style.paddingRight = 8;
            b.style.paddingTop = b.style.paddingBottom = 2;
            b.style.marginLeft = b.style.marginRight = b.style.marginTop = b.style.marginBottom = 0;
            b.AddToClassList("frame-ship-word");
            b.style.color = (Color)new Color32(236, 235, 230, 255);
            b.style.backgroundColor = (Color)new Color32(38, 26, 44, 235);
            b.style.borderLeftWidth = b.style.borderRightWidth = b.style.borderTopWidth = b.style.borderBottomWidth = 2;
            Color edge = new Color32(140, 106, 142, 255);
            b.style.borderLeftColor = b.style.borderRightColor = b.style.borderTopColor = b.style.borderBottomColor = edge;
            b.style.borderTopLeftRadius = b.style.borderTopRightRadius = b.style.borderBottomLeftRadius = b.style.borderBottomRightRadius = 8;
            return b;
        }

        /// <summary>Changes the places of the chart and the 3D view (on), or puts them back (off).</summary>
        void Apply(bool on)
        {
            var map = MapView.Current;
            if (on)
            {
                mapDoc = map != null ? map.GetComponent<UIDocument>() : null;
                if (mapDoc == null || frameDoc == null || mapDoc.rootVisualElement == null)
                    return;
                miniMap = map;
                mapRoot = mapDoc.rootVisualElement;
                mapOrder = mapDoc.sortingOrder;
                mapDoc.sortingOrder = frameDoc.sortingOrder + 0.5f;
                mapRoot.style.overflow = Overflow.Hidden;
                mapRoot.style.borderTopLeftRadius = mapRoot.style.borderTopRightRadius = mapRoot.style.borderBottomLeftRadius = mapRoot.style.borderBottomRightRadius = 9;
                swapMini = null;                                          // (the one Swap button, in the top bar, puts them back)
                mapScale = map.chartScale;
                map.chartScale = mapScale * Mathf.Max(1f, miniChartScale);
                if (big != null) big.style.display = DisplayStyle.Flex;
                if (small != null) small.style.display = DisplayStyle.None;

            }
            else
            {
                swapMini?.RemoveFromHierarchy();
                swapMini = null;
                if (mapDoc != null && mapRoot != null)
                {
                    mapDoc.sortingOrder = mapOrder;
                    mapRoot.style.left = 0;                               // (as MapView.OnEnable set them)
                    mapRoot.style.top = 0;
                    mapRoot.style.right = 0;
                    mapRoot.style.bottom = 0;
                    mapRoot.style.width = StyleKeyword.Auto;
                    mapRoot.style.height = StyleKeyword.Auto;
                    mapRoot.style.overflow = Overflow.Visible;
                    mapRoot.style.borderTopLeftRadius = mapRoot.style.borderTopRightRadius = mapRoot.style.borderBottomLeftRadius = mapRoot.style.borderBottomRightRadius = 0;
                }
                if (miniMap != null && mapScale > 0f)
                    miniMap.chartScale = mapScale;
                miniMap = null;
                mapDoc = null;
                mapRoot = null;
                if (big != null) big.style.display = DisplayStyle.None;
                if (small != null) small.style.display = DisplayStyle.Flex;
                if (swapSmall != null) swapSmall.style.display = DisplayStyle.Flex;
            }
            applied = on;
            if (map != null)
                map.Follow();                                             // (the chart's view goes back to her at its own scale: the window changed size)
        }

        /// <summary>The screen's size in pixels as the frame's panel is drawn (a test draws the panel into a texture of its own).</summary>
        Vector2Int ScreenPixels()
        {
            var target = frameDoc != null && frameDoc.panelSettings != null ? frameDoc.panelSettings.targetTexture : null;
            return target != null ? new Vector2Int(target.width, target.height) : new Vector2Int(Screen.width, Screen.height);
        }

        /// <summary>The texture's size for where it is shown now, and (swapped) the chart kept to the ship window's rectangle.</summary>
        void Layout(FrameRoot frame)
        {
            Rect all = frame.Root.worldBound, win = shipView.LocalToWorld(shipView.contentRect);
            var view = stage.View;
            var screen = ScreenPixels();
            if ((Showroom || TitleSea) && !applied)
            {
                // (the whole screen is hers, whatever the frame shows or hides, and nothing of the chart is moved)
                int lines = ShowroomLines > 0 ? Mathf.Min(ShowroomLines, screen.y) : screen.y;
                int wide = Mathf.RoundToInt(lines * (screen.x / (float)Mathf.Max(1, screen.y)));
                view.every = 1;
                view.rate = kit.bigRate;
                view.ShownAt(wide, lines);
                view.filter = FilterMode.Bilinear;
                bool madeBig = !view.drawnByLook && view.Size(wide, lines);
                var shownNow = view.Picture;
                if (madeBig || small.image != shownNow)
                {
                    small.image = shownNow;
                    big.image = shownNow;
                    view.Render(true);
                }
                return;
            }
            if (all.width < 2f || win.width < 2f || win.height < 2f || float.IsNaN(win.width))
                return;
            float px = screen.x / all.width;                              // (screen pixels to one of the panel's own)
            // (riding HDRP's own waves a picture costs three times as much: fewer are drawn. ShipStageKit.smallRateRiding)
            bool riding = stage.SeaKind == StageSeaKind.HdrpWater && stage.Hdrp != null && stage.Hdrp.Rides;
            int w, h;
            if (applied)
            {
                int ask = view.bigHeight > 0 ? view.bigHeight : kit.bigHeight;
                h = ask > 0 ? Mathf.Min(ask, screen.y) : screen.y;
                w = Mathf.RoundToInt(h * (all.width / all.height));
                view.every = 1;
                view.rate = riding ? kit.bigRateRiding : kit.bigRate;
                mapRoot.style.left = win.x;
                mapRoot.style.top = win.y;
                mapRoot.style.width = win.width;
                mapRoot.style.height = win.height;
                mapRoot.style.right = StyleKeyword.Auto;
                mapRoot.style.bottom = StyleKeyword.Auto;
                view.ShownAt(screen.x, screen.y);
                // (the word for the look: over the log, at its left end, where no slip of the frame lies)
                var log = frame.Root.Q(null, "frame-log");
                if (log != null && wordBig != null && log.worldBound.width > 2f)
                {
                    wordBig.style.left = log.worldBound.x - all.x + 2f;
                    wordBig.style.bottom = all.yMax - log.worldBound.y + 8f;
                }
            }
            else
            {
                // (never more lines than the window has on this screen: they would only be thrown away)
                int lines = Mathf.Max(16, Mathf.RoundToInt(win.height * px));
                int ask = view.smallHeight > 0 ? view.smallHeight : kit.smallHeight;
                h = ask > 0 ? Mathf.Min(ask, lines) : lines;
                w = Mathf.RoundToInt(h * (win.width / win.height));
                view.every = Mathf.Max(1, kit.smallEvery);
                view.rate = riding ? kit.smallRateRiding : kit.smallRate;
                view.ShownAt(Mathf.RoundToInt(win.width * px), lines);
            }
            // (drawn smaller than it is shown, the stage's own picture is smoothed; a look's hard pixels are its own texture's)
            view.filter = FilterMode.Bilinear;
            bool made = !view.drawnByLook && view.Size(w, h);
            var picture = view.Picture;
            if (made || small.image != picture)
            {
                small.image = picture;
                big.image = picture;
                view.Render(true);                                        // (a new texture is empty until it is drawn)
            }
        }
    }
}
