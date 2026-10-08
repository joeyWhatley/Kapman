using System.Collections.Generic;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE COMMAND FRAME (the user, 7 Oct 2026: "I want the game to be more or less controlled via the frame UI, with
    /// intricate controls. I want the map to be more or less just a visual key"; Design/ui_mock/template3.html, layout G,
    /// "Thick rounded", "Map shows through"). On the chart scene's "Frame" object, its own UI document over the map's:
    ///   - THE TOP BAR: the menu, the date in plain words, the silver, the days of bread, what is owed, the clock's buttons;
    ///   - THE LEFT SIDE: windows that tile as i3 or sway has them (FrameWindows holds the state): four workspaces
    ///     (keys 1 to 4), each with its own windows as rows, columns or tabs (R, C, T), a new window (N), close (X),
    ///     focus by the arrows and by a click, the focused one edged in gold; a window's title steps to the next panel
    ///     or picks any registered one (FramePanels); under them the bar of workspaces and layouts;
    ///   - TOP RIGHT the portrait's place; BOTTOM RIGHT the ship window's (a drawn stand-in: FrameShipView);
    ///   - UNDER THE CHART the message log (FrameLog: the game's notices);
    ///   - and, in their own files, the floating windows for cards and talk (FrameRoot.Floats.cs) and the measure
    ///     (FrameRoot.Measure.cs).
    /// Where no window is, the chart shows and takes the mouse. While the frame is up the map's own side panels, its tool
    /// row and the port's menu in the middle stand down (MapView.Framed, TextPanel.External), and the dev keys that share
    /// the frame's letters are off (LevelClock.DevKeys: T; EconomyDevKeys.Keys: N). Its keys wait while a text box has the
    /// keyboard, while the title or the pause menu is up, and while a floating window wants an answer.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [DefaultExecutionOrder(900)]                                      // after the game's own updates, before the map draws (1000)
    public partial class FrameRoot : MonoBehaviour
    {
        public StyleSheet style;
        [Tooltip("Keep the player's arrangement of windows between runs (in the settings).")]
        public bool keepLayout = true;

        public static FrameRoot Current { get; private set; }
        /// <summary>Tests and tools: start from the workspaces a new player has, and keep nothing.</summary>
        public static bool FreshLayout;

        /// <summary>The windows' state: workspaces, layouts, panels, focus.</summary>
        public FrameWindows Windows { get; private set; }
        /// <summary>The frame's root element (the floating windows and the tools hang their own parts on it).</summary>
        public VisualElement Root => root;

        const string Pref = "kapman.frame.windows";

        VisualElement root, left, tiles, wmbar, who, shipBox, logBox, topBar, viewButtons;
        /// <summary>Where the buttons that change what is looked at stand, in the top bar by the clock's (the user's ruling
        /// of 7 Oct 2026, late: Chart or The port, Swap, Look). The port's picture and the ship's view put theirs here.</summary>
        public VisualElement ViewButtons => viewButtons;
        string floorClass;                                            // the floor under the text now on the root (TextFloor)
        PanelSettings panelSettings;
        float barHeight = -1f;
        Label date, part, silver, bread, debt, state, whoName, whoSay, shipWhere, shipWind;
        readonly Label[] whoLines = new Label[3];
        FrameFace face;
        FrameShipView shipView;
        ScrollView logScroll;
        readonly List<Button> speedButtons = new List<Button>();
        readonly List<FramePanel> shown = new List<FramePanel>();
        readonly List<VisualElement> windowEls = new List<VisualElement>();
        string drawnKey;
        int pick = -1;                                                // the window whose list of panels is open
        float nextFill, nextShip;
        string paletteClass;
        bool lyingKnown, wasLying, stoodDown;
        int logDrawn = -1, logDay = int.MinValue, heardThisFrame;
        string heardLast;
        bool menuWasUp;
        string keyNow;
        int keyVersion = -1, keyPick = -2, keyPanels = -1;
        int loadsTaken, keptVersion = -1;
        float nextKeep;
        int logStick;                                                 // frames the log is still held at its newest line after it was drawn

        void OnEnable()
        {
            Current = this;
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            // (ours from before, if this component alone was switched off and on: Build then takes a fresh copy of the layout)
            if (root != null && ReferenceEquals(root.userData, this))
                root.RemoveFromHierarchy();
            if (style != null && !docRoot.styleSheets.Contains(style))
                docRoot.styleSheets.Add(style);
            docRoot.style.position = Position.Absolute;
            docRoot.style.left = 0;
            docRoot.style.top = 0;
            docRoot.style.right = 0;
            docRoot.style.bottom = 0;
            docRoot.pickingMode = PickingMode.Ignore;

            Windows = new FrameWindows(() => FramePanels.Ids);
            // (a loaded game's arrangement first; else the player's own, from the settings; else a new player's)
            if (!(FrameSave.Layout.Length > 0 && Windows.Restore(FrameSave.Layout)) && keepLayout && !FreshLayout)
                Windows.Restore(PlayerPrefs.GetString(Pref, ""));
            loadsTaken = FrameSave.Loads;
            keptVersion = Windows.Version;
            lieTo = FrameSave.LieTo ?? "";
            foreach (var p in FramePanels.All)
                p.Frame = this;
            Build();
            UiTyping.Watch(root);
            drawnKey = null;
            keyNow = null;
            menuWasUp = false;
            lyingKnown = false;
            stoodDown = false;

            TextPanel.External = true;
            // the rulings of 7 Oct: only a card stops the clock (windows never do), and no hours are skipped (resting and
            // the tide wait keep the clock's own pace)
            PanelClock.OnlyCards = true;
            Rest.RateCap = 1f;
            TideWait.RateCap = 1f;
            LevelClock.DevKeys = false;
            EconomyDevKeys.Keys = false;
            Voyage.Notice += FrameLog.Add;
            GameSpeed.Changed += SpeedChanged;
            Known.Learned += Heard;
            // (a talk the graphs put up "for a choice, or so many seconds" counts the game's seconds here: it does not
            // answer itself while it holds the clock)
            TextPanel.WaitsByGameClock = true;
            FloatsEnable();
            EventsEnable();
        }

        void OnDisable()
        {
            Voyage.Notice -= FrameLog.Add;
            GameSpeed.Changed -= SpeedChanged;
            Known.Learned -= Heard;
            TextPanel.WaitsByGameClock = false;
            if (Windows != null)
                SaveState();                                             // (before the floating windows are let go)
            MeasureDisable();
            EventsDisable();
            FloatsDisable();
            foreach (var p in shown)
                p.Detach();
            shown.Clear();
            if (keepLayout && !FreshLayout && Windows != null)
            {
                PlayerPrefs.SetString(Pref, Windows.Capture());
                PlayerPrefs.Save();
            }
            TextPanel.External = false;
            PanelClock.Holds = null;
            PanelClock.LetGo = null;
            PanelClock.OnlyCards = false;
            Rest.RateCap = 0f;
            TideWait.RateCap = 0f;
            PanelClock.Waiting = null;
            VoyagePlanner.Moored = null;
            LevelClock.DevKeys = true;
            EconomyDevKeys.Keys = true;
            var map = MapView.Current;
            if (map != null)
                map.Framed = false;
            if (stoodDown && Notices.Current != null)
                Notices.Current.enabled = true;
            if (Current == this)
                Current = null;
        }

        /// <summary>The parts in their own files (the floating windows, the measure) come and go with the frame.</summary>
        partial void FloatsBuild();
        partial void FloatsEnable();
        partial void FloatsDisable();
        partial void FloatsUpdate();
        partial void MeasureBuild();
        partial void MeasureDisable();
        partial void MeasureUpdate();
        partial void RackUpdate();
        partial void JobsBuild();
        partial void JobsUpdate();
        partial void JobsPlaced();
        partial void JobsHead(VisualElement win);
        partial void MapToolsBuild();
        partial void MapToolsUpdate();
        /// <summary>The happenings of the events table, each a floating window (FrameRoot.Events.cs).</summary>
        partial void EventsEnable();
        partial void EventsDisable();
        partial void EventsUpdate();
        /// <summary>A new game asks who the player is (FrameRoot.Self.cs).</summary>
        partial void SelfUpdate();
        /// <summary>A part takes the keys this frame (a floating window open: the numbers are its choices).</summary>
        partial void FloatsKeys(Keyboard kb, ref bool taken);
        partial void MeasureKeys(Keyboard kb, ref bool taken);
        /// <summary>A part puts someone else in the portrait's place (whoever is speaking).</summary>
        partial void FloatsWho(ref WhoIs now);

        // ------------------------------------------------------------------ building

        /// <summary>The name of the frame's root element in its layout file.</summary>
        public const string RootName = "frame";

        /// <summary>
        /// THE FRAME IS LAID OUT IN A FILE, NOT BUILT HERE (KAPMAN, step 2; the user, 7 Oct 2026: "make it like people
        /// usually make it, not just all in script"; by the picker: UI Builder files). The file is the UI Document's
        /// own Source Asset, Game/UI/Frame.uxml: open it in UI Builder to move, resize or restyle any part. This finds
        /// each part by its name and gives it its words, its numbers and what a click on it does. A part may stand
        /// anywhere in the file and look like anything, but must keep its name: one that is gone is said so in the
        /// Console, and a plain stand-in is made so the game still runs.
        /// </summary>
        void Build()
        {
            var doc = GetComponent<UIDocument>();
            var docRoot = doc.rootVisualElement;
            // the document's own copy of the layout, made when it was switched on, if nobody has filled it yet; else a
            // fresh copy (this component alone was switched off and on)
            root = docRoot.Q<VisualElement>(RootName);
            if (root == null || root.userData != null)
            {
                root?.RemoveFromHierarchy();
                root = doc.visualTreeAsset != null ? doc.visualTreeAsset.Instantiate().Q<VisualElement>(RootName) : null;
                if (root == null)
                {
                    Debug.LogError("Kapman frame: the UI Document of '" + name + "' has no Source Asset with an element named \"" + RootName +
                                   "\" (it should be Assets/Kapman/Game/UI/Frame.uxml). The frame is empty.");
                    root = new VisualElement { name = RootName };
                    root.AddToClassList("frame-root");
                }
                root.RemoveFromHierarchy();
                docRoot.Add(root);
            }
            root.userData = this;                                         // (filled: it is not filled twice)
            root.pickingMode = PickingMode.Ignore;
            floorClass = null;                                            // (a new root has no floor class yet)
            barHeight = -1f;
            panelSettings = doc.panelSettings;

            // ---- the top bar
            topBar = Part<VisualElement>("top");
            Part<Button>("menu").clicked += () => MainMenu.Current?.Pause();
            date = Part<Label>("date");
            part = Part<Label>("part");
            silver = Part<Label>("silver");
            bread = Part<Label>("bread");
            debt = Part<Label>("debt");
            // the coin and the barrel: pictures of the art folder (with no coin's picture, the word "silver" stands)
            var coin = Part<Image>("coin");
            var coinPicture = FrameArt.Load("hud/hud.coin.png");
            coin.image = coinPicture;
            coin.scaleMode = ScaleMode.ScaleToFit;
            coin.style.display = coinPicture != null ? DisplayStyle.Flex : DisplayStyle.None;
            Part<Label>("silver-word").style.display = coinPicture != null ? DisplayStyle.None : DisplayStyle.Flex;
            var stores = Part<Image>("stores");
            var storesPicture = FrameArt.Load("hud/hud.stores.png");
            stores.image = storesPicture;
            stores.scaleMode = ScaleMode.ScaleToFit;
            stores.style.display = storesPicture != null ? DisplayStyle.Flex : DisplayStyle.None;
            viewButtons = Part<VisualElement>("views");
            speedButtons.Clear();
            Speed(Part<Button>("speed-pause"), -1, null);
            for (int i = 0; i < GameSpeed.Count; i++)
                Speed(Part<Button>("speed-" + (i + 1)), i, GameSpeed.Tips[i] + (i == 0 ? "  [-]" : "  [+]"));
            state = Part<Label>("state");

            // ---- the left side: the buttons, the window, and the bar under it
            left = Part<VisualElement>("left");
            BuildRack(left);                                              // (the function buttons, over the windows: FrameRoot.Rack.cs)
            JobsBuild();                                                  // (the list Before she sails, and the reason: FrameRoot.Jobs.cs)
            tiles = Part<VisualElement>("tiles");
            wmbar = Part<VisualElement>("wmbar");

            // ---- the portrait's place
            who = Part<VisualElement>("who");
            face = Part<FrameFace>("face");
            whoName = Part<Label>("who-name");
            for (int i = 0; i < whoLines.Length; i++)
                whoLines[i] = Part<Label>("who-line-" + (i + 1));
            whoSay = Part<Label>("who-say");

            // ---- the ship window's place
            shipBox = Part<VisualElement>("ship");
            shipView = Part<FrameShipView>("ship-view");
            shipWhere = Part<Label>("ship-where");
            shipWind = Part<Label>("ship-wind");

            // ---- the message log
            logBox = Part<VisualElement>("log");
            logScroll = Part<ScrollView>("log-scroll");
            logDrawn = -1;

            MeasureBuild();
            MapToolsBuild();
            FloatsBuild();
        }

        /// <summary>A part of the frame, by its name in the layout file. If the file has none such (taken out or renamed
        /// in UI Builder), that is said in the Console and a plain one is made, so the game still runs.</summary>
        T Part<T>(string partName) where T : VisualElement, new()
        {
            var e = root.Q<T>(partName);
            if (e != null)
                return e;
            Debug.LogError("Kapman frame: the layout (Frame.uxml) has no " + typeof(T).Name + " named \"" + partName +
                           "\". A plain one is made so the game runs: put the part back in UI Builder (it may stand anywhere, under that name).");
            e = new T { name = partName };
            root.Add(e);
            return e;
        }

        /// <summary>The rack of function buttons, at the top of the left column.</summary>
        partial void BuildRack(VisualElement column);

        static VisualElement Box(string cls)
        {
            var v = new VisualElement();
            v.AddToClassList(cls);
            return v;
        }

        static VisualElement Slip(string cls)
        {
            var v = Box("frame-slip");
            v.AddToClassList(cls);
            return v;
        }

        static Label Text(string cls)
        {
            var l = new Label();
            l.AddToClassList(cls);
            return l;
        }

        /// <summary>A button of the clock: Pause (level -1) or one of its speeds. `tip`: its hover words (null: the file's own).</summary>
        void Speed(Button b, int level, string tip)
        {
            b.clicked += () =>
            {
                if (level < 0)
                {
                    // (Pause pauses: while a card holds the clock it is stopped already, and Space or a speed sets it running)
                    if (GameSpeed.PlayerPaused || !PanelClock.Holding)
                        GameSpeed.TogglePause();
                }
                else
                    GameSpeed.SetLevel(level);
            };
            if (tip != null)
                b.tooltip = tip;
            b.userData = level;
            speedButtons.Add(b);
        }

        // ------------------------------------------------------------------ each frame

        void Update()
        {
            var menu = MainMenu.Current;
            bool title = menu != null && menu.OnTitle;
            var map = MapView.Current;
            // (the chest opens over the chart, on the map's own document under this one: the frame steps aside for it)
            root.style.display = title || (map != null && map.ChestOpen) ? DisplayStyle.None : DisplayStyle.Flex;
            if (map != null && map.chartMode)
                map.Framed = true;
            // no text under 18 true pixels: the sizes are lifted on a small screen (TextFloor), and what stands under the
            // top bar follows the bar when its words wrap and it grows
            if (TextFloor.Keep(root, panelSettings, ref floorClass))
                logStick = 6;                                             // (the lines are another size: the log is held at its newest again)
            UnderTheBar();
            if (!stoodDown && Notices.Current != null)
            {
                // the game's notices are the log's now: the fading lines over the chart stand down
                stoodDown = true;
                Notices.Current.enabled = false;
            }
            Palette();
            if (loadsTaken != FrameSave.Loads)
                LoadState();
            Workspace();
            KeepLayout();
            if (nextKeep > 0f && Time.realtimeSinceStartup >= nextKeep)
            {
                nextKeep = 0f;
                PlayerPrefs.Save();
            }
            Keys(menu);
            HeardFlush();

            string key = WindowsKey();
            if (key != drawnKey)
                DrawWindows(key);
            else
                FocusMarks();

            float now = Time.realtimeSinceStartup;                      // (the wall clock: the game's seconds stand still while a panel holds it)
            if (now >= nextFill)
            {
                nextFill = now + 0.25f;
                for (int i = 0; i < shown.Count; i++)
                    shown[i].Tick();
                FillTop();
                FillWho();
                FillShip();
                FillLog();
            }
            if (now >= nextShip)
            {
                nextShip = now + 1f / 15f;
                shipView.MarkDirtyRepaint();
            }
            if (logStick > 0)
            {
                logStick--;
                logScroll.scrollOffset = new Vector2(0f, Mathf.Max(0f, logScroll.verticalScroller.highValue));
            }
            RackUpdate();
            JobsUpdate();
            MapToolsUpdate();
            MeasureUpdate();
            SelfUpdate();
            if (KapmanGame.Current != null && FirstJobs.Tick(KapmanGame.Current.Day))
                FrameLog.Add("The sail is mended and bent on again. It will hold.");
            EventsUpdate();
            FloatsUpdate();
        }

        /// <summary>The left column, the portrait and the measure's slip stand a little under the top bar, whatever its
        /// height is now (one line of words at 1080p; two where a small screen makes them wrap).</summary>
        void UnderTheBar()
        {
            float h = topBar.resolvedStyle.height;
            if (float.IsNaN(h) || h < 1f || Mathf.Abs(h - barHeight) < 0.5f)
                return;
            barHeight = h;
            float y = h + 14f;
            left.style.top = y;                                           // (the portrait stands in the foot now: Frame.uss)
            MeasureTop(y);
            FloatsTop(h);
        }

        partial void MeasureTop(float y);
        /// <summary>The floating windows' own ground begins under the top bar (FrameRoot.Floats.cs).</summary>
        partial void FloatsTop(float y);

        /// <summary>The frame's edge and accent follow the world's palette (the player's people).</summary>
        void Palette()
        {
            string cls = FrameColours.Class(MapPalette.Current);
            if (cls == paletteClass)
                return;
            if (paletteClass != null)
                root.RemoveFromClassList(paletteClass);
            paletteClass = cls;
            root.AddToClassList(cls);
            face.Who = "";                                               // (the player's portrait is their people's: looked up again)
            nextFill = 0f;
        }

        /// <summary>She moors: the Port workspace comes up. She casts off: the Sea workspace. Only at the change, so the
        /// player's own choice stands in between.</summary>
        void Workspace()
        {
            var game = KapmanGame.Current;
            if (game == null || game.ship == null)
                return;
            bool lying = FrameGame.PortHere != null;
            if (lyingKnown && lying == wasLying)
                return;
            // (a new game: wait a moment for the port's graph to lay her to before taking her for at sea)
            if (!lyingKnown && !lying && Time.timeSinceLevelLoad < 1.5f)
                return;
            lyingKnown = true;
            wasLying = lying;
            pick = -1;
            Windows.Go(lying ? FrameWindows.PortSpace : FrameWindows.SeaSpace);
        }

        // ------------------------------------------------------------------ the keys

        void Keys(MainMenu menu)
        {
            var kb = Keyboard.current;
            // (the menu's own keys are numbers and Esc, and it looks at them before the frame does in the same frame. So the
            // key that shuts the menu is not the frame's: without this, Esc to resume opened the menu again at once, and
            // "1. Resume" also brought up workspace 1)
            bool menuUp = menu != null && (menu.Open || menu.OnTitle);
            bool menuJustShut = menuWasUp && !menuUp;
            menuWasUp = menuUp;
            if (kb == null || UiTyping.Active || menuUp || menuJustShut || PlayNotes.Busy || Showtime.Busy)
                return;
            if (kb.ctrlKey.isPressed || kb.altKey.isPressed)
                return;
            bool taken = false;
            FloatsKeys(kb, ref taken);
            if (!taken)
                MeasureKeys(kb, ref taken);
            if (taken)
                return;
            var map = MapView.Current;
            if (map != null && map.ChestOpen)
                return;                                                   // (the chest is open over the frame: its own keys)
            if (FrameWindows.OneWindow)
            {
                // (one window on the side: no workspaces to go to, and nothing to tile, add or close)
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    if (pick >= 0)
                        pick = -1;
                    else if (map != null && map.ToolInHand)
                        map.PutToolDown();
                    else
                        menu?.Pause();
                }
                return;
            }
            for (int i = 0; i < 4; i++)
                if (kb[Key.Digit1 + i].wasPressedThisFrame || kb[Key.Numpad1 + i].wasPressedThisFrame)
                {
                    pick = -1;
                    Windows.Go(i);
                    return;
                }
            if (kb.rKey.wasPressedThisFrame) Windows.SetLayout(FrameLayout.Rows);
            else if (kb.cKey.wasPressedThisFrame) Windows.SetLayout(FrameLayout.Columns);
            else if (kb.tKey.wasPressedThisFrame) Windows.SetLayout(FrameLayout.Tabs);
            else if (kb.nKey.wasPressedThisFrame) { pick = -1; Windows.Add(); }
            else if (kb.xKey.wasPressedThisFrame) { pick = -1; Windows.Close(Windows.Space.focus); }
            else if (kb.downArrowKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) Windows.FocusBy(1);
            else if (kb.upArrowKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) Windows.FocusBy(-1);
            else if (kb.escapeKey.wasPressedThisFrame)
            {
                // Esc, when no floating window or tool took it: the list of panels shuts, a map tool is put down, else the menu
                if (pick >= 0)
                    pick = -1;
                else if (map != null && map.ToolInHand)
                    map.PutToolDown();
                else
                    menu?.Pause();
            }
        }

        // ------------------------------------------------------------------ the windows

        /// <summary>What the left side is built from: the workspace, its layout and panels, the open list of panels; in
        /// tabs the focus too (only the focused window is there). A change of focus alone only moves the gold edge.</summary>
        string WindowsKey()
        {
            // (asked every frame: put together again only when something it is made of has moved)
            int panels = FramePanels.Ids.Count;
            if (keyNow != null && Windows.Version == keyVersion && pick == keyPick && panels == keyPanels)
                return keyNow;
            keyVersion = Windows.Version;
            keyPick = pick;
            keyPanels = panels;
            var w = Windows.Space;
            keyNow = Windows.Current + "/" + (int)w.layout + "/" + string.Join(",", w.slots) + "/" + pick + (w.layout == FrameLayout.Tabs ? "/" + w.focus : "") +
                     "/" + panels + "/" + SizesKey(w);
            return keyNow;
        }

        /// <summary>The sizes of the windows of a workspace, as part of what the left side is built from.</summary>
        static string SizesKey(FrameWorkspace w)
        {
            if (w.sizes.Count == 0)
                return "";
            var b = new System.Text.StringBuilder();
            foreach (string id in w.slots)
                b.Append(w.sizes.TryGetValue(id, out int size) ? size : FrameWindows.Medium);
            return b.ToString();
        }

        void DrawWindows(string key)
        {
            drawnKey = key;
            foreach (var p in shown)
                p.Detach();
            shown.Clear();
            windowEls.Clear();
            tiles.Clear();
            var w = Windows.Space;
            bool wide = w.layout == FrameLayout.Columns && w.slots.Count > 1;
            left.EnableInClassList("wide", wide);
            root.EnableInClassList("wide", wide);
            tiles.EnableInClassList("cols", w.layout == FrameLayout.Columns);
            if (w.layout == FrameLayout.Tabs)
            {
                var tabs = Box("frame-tabs");
                for (int i = 0; i < w.slots.Count; i++)
                {
                    int index = i;
                    var tab = new Button(() => { pick = -1; Windows.Focus(index); }) { text = FramePanels.Title(w.slots[i]) };
                    tab.AddToClassList("frame-tab");
                    tab.EnableInClassList("on", i == w.focus);
                    tabs.Add(tab);
                }
                tiles.Add(tabs);
                if (w.slots.Count > 0)
                    tiles.Add(Window(w, w.focus, true));
            }
            else
                for (int i = 0; i < w.slots.Count; i++)
                    tiles.Add(Window(w, i, i == w.slots.Count - 1));

            // the bar: the workspaces, then the layouts and a new window (not there with one window on the side)
            wmbar.style.display = FrameWindows.OneWindow ? DisplayStyle.None : DisplayStyle.Flex;
            wmbar.Clear();
            for (int i = 0; i < Windows.spaces.Count; i++)
            {
                int index = i;
                var b = new Button(() => { pick = -1; Windows.Go(index); }) { text = (i + 1) + " " + Windows.spaces[i].name, tooltip = "Workspace " + (i + 1) + "  [" + (i + 1) + "]" };
                b.AddToClassList("frame-wm");
                b.EnableInClassList("on", i == Windows.Current);
                wmbar.Add(b);
            }
            wmbar.Add(Box("frame-wm-space"));
            foreach (var (layout, name, letter) in new[] { (FrameLayout.Rows, "Rows", "R"), (FrameLayout.Columns, "Columns", "C"), (FrameLayout.Tabs, "Tabs", "T") })
            {
                var l = layout;
                var b = new Button(() => Windows.SetLayout(l)) { text = name, tooltip = name + "  [" + letter + "]" };
                b.AddToClassList("frame-wm");
                b.EnableInClassList("on", w.layout == layout);
                wmbar.Add(b);
            }
            if (w.slots.Count < FrameWindows.MaxWindows && w.slots.Count < FramePanels.Ids.Count)
            {
                var add = new Button(() => { pick = -1; Windows.Add(); }) { tooltip = "A new window  [N]" };
                add.AddToClassList("frame-wm");
                add.Add(new FrameGlyph(FrameGlyph.Kind.Plus));
                wmbar.Add(add);
            }
            JobsPlaced();                                                 // (the list of the first jobs goes into the new window at once)
        }

        VisualElement Window(FrameWorkspace w, int index, bool last)
        {
            string id = w.slots[index];
            var panel = FramePanels.Get(id);
            // (laid out in Game/UI/Parts/Window.uxml)
            var parts = FrameParts.Current;
            var win = FrameParts.Make<VisualElement>(parts != null ? parts.window : null, "window");
            win.EnableInClassList("focus", index == w.focus);
            win.EnableInClassList("last", last);
            win.AddToClassList("size-" + Windows.SizeOf(index));         // (small 0, medium 1, large 2: the look of each is the brief faces' builder's)
            win.userData = index;
            // a click anywhere in a window gives it the focus (the buttons under the click still get it)
            win.RegisterCallback<PointerDownEvent>(_ => Windows.Focus(index), TrickleDown.TrickleDown);

            // (while the tab of the first jobs' list is the one shown, the window's own name and arrows are the other
            // tab: a click on them turns to the window, and only the next click on the name opens the list of panels)
            FrameParts.In<Button>(win, "before").clicked += () => { pick = -1; TodoShow(false); Windows.Step(index, -1); };
            FrameParts.In<Button>(win, "title").clicked += () =>
            {
                if (TodoOn)
                {
                    TodoShow(false);
                    return;
                }
                pick = pick == index ? -1 : index;
            };
            FrameParts.In<Label>(win, "name").text = panel != null ? panel.Title : id;
            FrameParts.In<Button>(win, "next").clicked += () => { pick = -1; TodoShow(false); Windows.Step(index, 1); };
            JobsHead(win);                                                // (the tab "To do": FrameRoot.Jobs.cs)
            // (the cross is kept for the key and for a look that shows it; the look of 7 Oct 2026 puts it away: "not so
            // many X's". With one window there is nothing to close.)
            var shut = FrameParts.In<Button>(win, "shut");
            if (w.slots.Count > 1)
                shut.clicked += () => { pick = -1; Windows.Close(index); };
            else
                shut.RemoveFromHierarchy();

            var scroll = FrameParts.In<ScrollView>(win, "scroll");
            if (pick == index)
            {
                var list = Box("frame-picker");
                foreach (var p in FramePanels.All)
                {
                    string to = p.Id;
                    var b = new Button(() => { pick = -1; Windows.Set(index, to); }) { text = p.Title };
                    b.AddToClassList("frame-pick");
                    b.EnableInClassList("on", to == id);
                    b.EnableInClassList("used", to != id && w.slots.Contains(to));
                    list.Add(b);
                }
                scroll.Add(list);
            }
            else if (panel != null)
            {
                var body = Box("frame-body");
                scroll.Add(body);
                panel.Frame = this;
                panel.Attach(body);
                shown.Add(panel);
            }
            windowEls.Add(win);
            return win;
        }

        /// <summary>The gold edge follows the focus without the windows being built again (so a click that gives a window
        /// the focus still reaches the button under it).</summary>
        void FocusMarks()
        {
            int focus = Windows.Space.focus;
            foreach (var win in windowEls)
                win.EnableInClassList("focus", (int)win.userData == focus);
        }

        /// <summary>Brings a panel up (the workspace that shows it, and the focus on it): for a button that says "show
        /// the Voyage window".</summary>
        public void Show(string panelId)
        {
            pick = -1;
            Windows.Show(panelId);
        }

        // ------------------------------------------------------------------ the saved game (FrameSave)

        partial void FloatsCapture();
        partial void FloatsLoaded();

        /// <summary>The frame's state as it is now, for the saved game (FrameSave asks).</summary>
        public void SaveState()
        {
            // (a game just loaded that the frame has not taken up yet, a save made in the same frame: what was loaded
            // is what is saved, not the windows of the game before)
            if (loadsTaken != FrameSave.Loads)
                return;
            if (Windows != null)
                FrameSave.Layout = Windows.Capture();
            FrameSave.LieTo = lieTo ?? "";
            FloatsCapture();
        }

        /// <summary>A game has been loaded under the frame: its arrangement over the one on screen, and everything that
        /// was of the game before put away (its windows over the chart, the measure, a course drawn for a window).</summary>
        void LoadState()
        {
            loadsTaken = FrameSave.Loads;
            if (FrameSave.Layout.Length > 0 && Windows.Restore(FrameSave.Layout))
            {
                pick = -1;
                drawnKey = null;
                keyNow = null;
                keptVersion = Windows.Version;                           // (a loaded game's arrangement is not written to the settings until the player changes it)
            }
            lieTo = FrameSave.LieTo ?? "";
            SetMeasuring(false);
            ShowPlanned(null);
            foreach (var p in FramePanels.All)
                p.LoadedGame();                                          // (what each kept of the game before: dropped)
            MapView.Current?.Follow();
            FloatsLoaded();
            // (the saved workspace stands: she lies where the loaded game has her, and that is not a mooring or a casting off)
            wasLying = FrameGame.PortHere != null;
            lyingKnown = true;
            logDrawn = -1;
            nextFill = 0f;
            nextRack = 0f;
        }

        /// <summary>The settings follow every change of the windows (not only a clean close: a crash lost the player's
        /// arrangement), written to disk a few seconds after the last change.</summary>
        void KeepLayout()
        {
            if (!keepLayout || FreshLayout || Windows.Version == keptVersion)
                return;
            keptVersion = Windows.Version;
            PlayerPrefs.SetString(Pref, Windows.Capture());
            nextKeep = Time.realtimeSinceStartup + 3f;
        }

        void OnApplicationQuit()
        {
            if (keepLayout && !FreshLayout && Windows != null)
            {
                PlayerPrefs.SetString(Pref, Windows.Capture());
                PlayerPrefs.Save();
            }
        }

        // ------------------------------------------------------------------ the top bar

        /// <summary>The speed or the pause changed (a button, a key, or the clock dropped back by itself): the bar says so at once.</summary>
        void SpeedChanged() => nextFill = 0f;

        void FillTop()
        {
            var game = KapmanGame.Current;
            var clock = LevelClock.Current;
            if (clock != null)
            {
                // (the day as it was kept in 793: its feast, or reckoned by the Kalends, Nones and Ides: PeriodDate)
                date.text = PeriodDate.Words(clock.Date);
                date.tooltip = PeriodDate.Tip(clock.Date);
                // (at speed 5 a part of the day lasts under a second: the date alone is told)
                part.text = !GameSpeed.Paused && GameSpeed.Running >= 4 ? "" : FrameUi.PartOfDay(clock.HourOfDay);
            }
            if (Trade.Started)
            {
                silver.text = $"{FrameGame.Silver:0} d";
                // (what the purse buys where she lies: a new game's 86 d is 40 deniers of coin and three other silvers)
                var here = FrameGame.PortHere;
                double buys = FrameGame.SilverHere(here);
                silver.parent.tooltip = "Your silver, in deniers' worth: all your coin and cut silver together." +
                                        (here != null && FrameGame.Silver - buys > 0.5 ? $" At {here.name} it buys {buys:0} d's worth: coin of other lands and cut silver go at a loss." : "");
                string owed = FrameGame.DebtLine();
                debt.text = owed.Length > 0 ? owed : "nothing owed";
                debt.EnableInClassList("warn", game != null && Trade.Debt > 0.01 && Trade.DebtDaysLeft(game.Day) < 14.0);
                string sunday = FrameGame.DebtDayWarning();
                debt.parent.tooltip = "What you owe, and the day it falls due (" + FrameGame.DebtDay(true) + ")." + (sunday.Length > 0 ? " " + sunday : "");
            }
            if (Voyage.Started)
            {
                double days = Voyage.FoodDays;
                // (the user, 8 Oct 2026: a barrel, the word, "and it just says the days left")
                bread.text = days < 0.25 ? "none left" : FrameUi.Days(days);
                bread.EnableInClassList("warn", days < 2.0);
            }
            foreach (var b in speedButtons)
            {
                int level = (int)b.userData;
                b.EnableInClassList("on", level < 0 ? GameSpeed.PlayerPaused : !GameSpeed.PlayerPaused && GameSpeed.Level == level);
                b.EnableInClassList("was", level >= 0 && level == GameSpeed.Before);   // (the speed she ran at before the clock dropped back)
                b.EnableInClassList("capped", level > GameSpeed.Cap);                  // (a pressing matter: no faster than the cap for now)
            }
            // (after the clock dropped back by itself: why, and the one key that goes back to the speed she had)
            bool dropped = !GameSpeed.Paused && GameSpeed.Before > GameSpeed.Level;
            state.text = GameSpeed.Paused ? (GameSpeed.PlayerPaused || GameSpeed.HeldFor("menu") ? "paused" : PanelClock.Holding ? "waits for your answer"
                                             : GameSpeed.HeldFor("alongside") ? "alongside: the clock stands" : "held")
                       : dropped ? "slowed" + (GameSpeed.DroppedFor.Length > 0 ? ", " + GameSpeed.DroppedFor : "") + ". [+] back to " + (GameSpeed.Before + 1)
                       : GameSpeed.Level > GameSpeed.Cap ? "no faster than " + (GameSpeed.Cap + 1) + " until it is answered" : "";
            // (it hangs under the bar's right end as a small tag, and is not there when it has nothing to say: the bar
            // itself has the view buttons in that place now)
            state.style.display = state.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            state.tooltip = PanelClock.Holding && !GameSpeed.PlayerPaused ? "A card holds the clock. Answer it, or press Space or a speed to let the clock run while it waits."
                          : dropped ? "The clock slowed by itself. Press + to go back to the speed you had. Each reason has a switch: Menu, The clock." : "";
        }

        // ------------------------------------------------------------------ the portrait's place

        /// <summary>Who stands in the portrait's place: a name, up to three lines about them, and what they are saying ("" for none).</summary>
        public struct WhoIs
        {
            public string name, line1, line2, line3, say, face;
        }

        void FillWho()
        {
            var game = KapmanGame.Current;
            var home = GameData.Port(ChartWorld.Current != null ? ChartWorld.Current.homePort : "dorestad");
            // (who she is, once chosen: her people and home, her faith; the ship has its own window now)
            var people = Self.Chosen ? SelfBook.Current.People(Self.People) : null;
            var faith = Self.Chosen ? SelfBook.Current.Faith(Self.Faith) : null;
            // (the user, 7 Oct 2026, late: "when it's just focused back on the player's character just show name, stats,
            // culture, religion". The crew has its own window.)
            string stats = "";
            if (Self.Chosen)
                foreach (string id in Self.StatIds)
                    stats += (stats.Length > 0 ? "  " : "") + (SelfBook.Current.Stat(id)?.name ?? id) + " " + Self.Stat(id);
            var now = new WhoIs
            {
                name = Self.Chosen && Self.Called.Length > 0 ? Self.Called : "You",
                line1 = stats,
                line2 = people != null ? people.name + (home != null ? ", of " + home.name : "") : home != null ? "A merchant of " + home.name : "A merchant",
                line3 = faith != null ? faith.name
                      : game != null && game.ship != null && game.ship.shipClass != null ? "The " + game.ship.shipClass.shipName : "",
                say = "", face = "you",
            };
            FloatsWho(ref now);
            whoName.text = now.name ?? "";
            Line(whoLines[0], now.line1);
            Line(whoLines[1], now.line2);
            Line(whoLines[2], now.line3);
            Line(whoSay, now.say);
            face.Who = now.face ?? now.name ?? "";
        }

        static void Line(Label l, string text)
        {
            l.text = text ?? "";
            l.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        // ------------------------------------------------------------------ the ship window's place

        void FillShip()
        {
            var game = KapmanGame.Current;
            var clock = LevelClock.Current;
            var here = FrameGame.PortHere;
            bool river = ChartWorld.Current != null && ChartWorld.OnRiver();
            var look = new FrameShipView.Look
            {
                place = here != null ? FrameShipView.Place.Port : river ? FrameShipView.Place.River : FrameShipView.Place.Sea,
                night = LevelClock.Night, golden = LevelClock.Golden, sunAltitude = LevelClock.SunAltitude,
                hour = clock != null ? clock.HourOfDay : 12f,
                lying = game == null || game.ship == null || game.ship.anchored,
            };
            float from = 0f;
            if (ChartWorld.TryWeather(out var air))
            {
                look.wind = air.windSpeed;
                look.fog = air.fog;
                look.rain = Mathf.Clamp01(air.rain / 4f);
                from = air.windFrom;
                look.windOffBow = Mathf.DeltaAngle(game.ship.Heading, from);
                shipWind.text = FrameGame.WindWords(air.windSpeed, from);
            }
            else
                shipWind.text = "";
            shipView.State = look;
            var body = ChartBody.Current;
            string bound = Course.IsSet ? GameData.Port(Course.Destination)?.name : null;
            shipWhere.text = here != null ? "At " + here.name
                           : body != null && body.Sheltering ? "Lying to in the gale"
                           : look.lying ? (river ? "Lying to on the river" : "Lying to")
                           : (river ? "On the river" : "At sea") + (bound != null ? ", bound for " + bound : "");
        }

        // ------------------------------------------------------------------ the log

        /// <summary>Word heard goes in the log: one piece as it is; many at once (the waterfront's talk on arriving) as one line.</summary>
        void Heard(Fact f)
        {
            if (f == null || string.IsNullOrEmpty(f.text) || !Known.HeardNews(f.refId))
                return;
            heardThisFrame++;
            heardLast = "Word comes" + (string.IsNullOrEmpty(f.from) ? "" : " from " + f.from) + ": " + f.text;
        }

        void HeardFlush()
        {
            if (heardThisFrame == 1)
                FrameLog.Add(heardLast);
            else if (heardThisFrame > 1)
                FrameLog.Add($"You hear the talk of the waterfront: {heardThisFrame} things new to you. The Chronicle has them.");
            heardThisFrame = 0;
        }

        void FillLog()
        {
            var game = KapmanGame.Current;
            int today = game != null ? (int)System.Math.Floor(game.Day) : 0;
            if (logDrawn == FrameLog.Version && logDay == today)
                return;
            logDrawn = FrameLog.Version;
            logDay = today;
            logScroll.Clear();
            var logParts = FrameParts.Current;                             // (a line is laid out in Game/UI/Parts/LogLine.uxml)
            int from = Mathf.Max(0, FrameLog.Lines.Count - 40);
            for (int i = from; i < FrameLog.Lines.Count; i++)
            {
                var l = FrameLog.Lines[i];
                var row = FrameParts.Make<VisualElement>(logParts != null ? logParts.logLine : null, "log-line");
                row.EnableInClassList("last", i == FrameLog.Lines.Count - 1);
                row.EnableInClassList("slip", l.kind == "slip");          // (a landing's slip: framed)
                string when = (int)System.Math.Floor(l.day) == today ? l.part : l.date;
                FrameParts.In<Label>(row, "stamp").text = when.Length > 0 ? char.ToUpperInvariant(when[0]) + when.Substring(1) : "";
                FrameParts.In<Label>(row, "text").text = l.text;
                logScroll.Add(row);
            }
            if (FrameLog.Lines.Count == 0)
            {
                var row = FrameParts.Make<VisualElement>(logParts != null ? logParts.logLine : null, "log-line");
                FrameParts.In<Label>(row, "stamp").RemoveFromHierarchy();
                FrameParts.In<Label>(row, "text").text = "Nothing said yet.";
                logScroll.Add(row);
            }
            // (down to the newest, once the lines have their places: for the next few frames, in Update. Setting the place
            // once, or a moment later, was not sure: the scroller did not always know its new length yet, and the newest
            // line, a deal done or a course set, stayed out of sight below)
            logStick = 12;
        }
    }
}
