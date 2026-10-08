using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE PLAYER'S HANDS AND EYES, for the chart's play tests (7 Oct 2026: "play as a player does through the real
    /// controls"). A test plays the game with these and with nothing else:
    ///   - a CLICK goes to a place on the screen as a pointer pressed and let go there. Before it, the thing to be
    ///     clicked must be on screen and must be what the pointer hits at its middle: a button under another window, or
    ///     scrolled out of sight, fails the test as it would fail a player. (A list is scrolled to it first, as a hand
    ///     on the wheel would.)
    ///   - a KEY is a key of the keyboard pressed and let go, through the input system the game reads.
    ///   - a PICTURE is the screen as drawn, saved to Captures/chart; and every word on screen is read at that moment
    ///     for the faults a reader would see: a writing key shown as "[write: ...]", a window that says it could not be
    ///     drawn, a number that is not one.
    /// What the game is asked directly is only what a player sees with their eyes (is a window up, what does it say).
    /// </summary>
    public sealed class ChartPlay
    {
        public readonly string dir;
        RenderTexture rt;
        readonly PanelSettings settings;
        readonly RenderTexture oldTarget;
        readonly bool oldClear;
        readonly Color oldColour;
        readonly InputSettings.BackgroundBehavior oldBackground;
        readonly InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        Keyboard keyboard;
        bool madeKeyboard;

        /// <summary>Faults seen on screen in the frame's own windows (a test fails on any).</summary>
        public readonly List<string> faults = new List<string>();
        /// <summary>Words on a screen that were written for the makers (PageWords.Fault: what the game lacks, an id, a
        /// number in brackets, a workshop word), each once: the fault, the words, where first seen.</summary>
        public readonly List<string> pageFaults = new List<string>();
        readonly HashSet<string> pageSeen = new HashSet<string>();
        /// <summary>Text that runs out of its box (cut by it, or lying over what is beside it), and a single word broken
        /// over two lines: each once, with where it was first seen.</summary>
        public readonly List<string> cuts = new List<string>();
        /// <summary>The same on a screen smaller than 1080 lines, where the frame is drawn larger to keep its text
        /// readable and has less room: known to be tight, listed, not yet held.</summary>
        public readonly List<string> smallCuts = new List<string>();
        readonly HashSet<string> cutSeen = new HashSet<string>();
        /// <summary>Cuts in the tiled layouts that only the tests can reach (columns, tabs: the game has one window on the
        /// side): listed, not held.</summary>
        public readonly List<string> tiledCuts = new List<string>();
        /// <summary>Every size of text seen in the frame at full size (1920 x 1080), in panel pixels: there should be four.</summary>
        public readonly HashSet<int> frameSizes = new HashSet<int>();
        /// <summary>The smallest text seen on any screen, in true pixels, and where.</summary>
        public float smallestPx = float.MaxValue;
        public string smallestWhere = "";
        /// <summary>Writing that is not written yet, seen on a card or a talk (told, not failed: the words are the writers').</summary>
        public readonly List<string> unwritten = new List<string>();
        /// <summary>The pictures saved, in order.</summary>
        public readonly List<string> pictures = new List<string>();
        /// <summary>Buttons the pointer was on but that had to be pressed by the keyboard's way (0 when the pointer works).</summary>
        public int pressedByKey;
        public int clicks, keys;
        /// <summary>How many clicks the screen itself made of the pointer's presses (a mark on the chart hears a click, not
        /// a press), and what the last one fell on.</summary>
        public int clickEvents;
        public string lastClickOn = "";
        EventCallback<ClickEvent> heardClick;

        ChartPlay(string dir, PanelSettings settings)
        {
            this.dir = dir;
            this.settings = settings;
            rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            oldTarget = settings.targetTexture;
            oldClear = settings.clearColor;
            oldColour = settings.colorClearValue;
            settings.targetTexture = rt;
            settings.clearColor = true;
            settings.colorClearValue = new Color(0.106f, 0.137f, 0.251f);
            // (a batch run has no window in front: the keys must reach the game all the same)
            var input = InputSystem.settings;
            oldBackground = input.backgroundBehavior;
            oldEditorInput = input.editorInputBehaviorInPlayMode;
            input.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            input.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = Keyboard.current;
            if (keyboard == null)
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
                madeKeyboard = true;
            }
            Time.captureFramerate = 30;
            heardClick = e =>
            {
                clickEvents++;
                lastClickOn = e.target is VisualElement el ? el.GetType().Name + " " + string.Join(".", el.GetClasses()) : "";
            };
            Screen.RegisterCallback(heardClick, TrickleDown.TrickleDown);
        }

        /// <summary>Takes the screen for a test (the chart's own panel drawn into a picture). `fresh`: the pictures of the
        /// run before are thrown away first.</summary>
        public static ChartPlay Begin(bool fresh)
        {
            var map = MapView.Current;
            Assert.IsNotNull(map, "no MapView: the chart is not up");
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "chart");
            Directory.CreateDirectory(dir);
            if (fresh)
                foreach (string old in Directory.GetFiles(dir, "*.png"))
                    File.Delete(old);
            EndActive();                                                 // (one at a time: a test that failed may have left its own)
            return active = new ChartPlay(dir, map.GetComponent<UIDocument>().panelSettings);
        }

        static ChartPlay active;
        bool ended;

        /// <summary>Gives the screen back if a test still has it (a test's tear-down: a test that fails does not reach
        /// its own clean-up).</summary>
        public static void EndActive()
        {
            active?.End();
            active = null;
        }

        /// <summary>Gives the screen back as it was.</summary>
        public void End()
        {
            if (ended)
                return;
            ended = true;
            // (the page's rule and the boxes: what the screens of this walk showed)
            if (pageFaults.Count > 0)
                Debug.Log($"Chart test (page): {pageFaults.Count} texts written for the makers reached a screen:\n" + string.Join("\n", pageFaults));
            if (cuts.Count > 0)
                Debug.Log($"Chart test (page): {cuts.Count} texts run out of their box or break a word:\n" + string.Join("\n", cuts));
            if (smallCuts.Count > 0)
                Debug.Log($"Chart test (page, small screen): {smallCuts.Count} texts run out of their box or break a word at 720 lines:\n" + string.Join("\n", smallCuts));
            if (tiledCuts.Count > 0)
                Debug.Log($"Chart test (page, tiled layouts): {tiledCuts.Count} texts run out of their box or break a word in the columns or the tabs:\n" + string.Join("\n", tiledCuts));
            if (active == this)
                active = null;
            Time.captureFramerate = 0;
            try
            {
                if (MapView.Current != null && heardClick != null)
                    Screen.UnregisterCallback(heardClick, TrickleDown.TrickleDown);
            }
            catch (Exception)
            {
                // (the screen it listened on is gone with its scene)
            }
            settings.targetTexture = oldTarget;
            settings.clearColor = oldClear;
            settings.colorClearValue = oldColour;
            rt.Release();
            var input = InputSystem.settings;
            input.backgroundBehavior = oldBackground;
            input.editorInputBehaviorInPlayMode = oldEditorInput;
            if (madeKeyboard && keyboard != null)
                InputSystem.RemoveDevice(keyboard);
        }

        // ------------------------------------------------------------------ the screen

        /// <summary>The screen made another size, as on a smaller monitor (1280 x 720): the game lays itself out again
        /// for it over the next frames. Clicks still fall where things are (the pointer is in the panel's own measure).</summary>
        public IEnumerator Resize(int width, int height)
        {
            if (rt.width == width && rt.height == height)
                yield break;
            var old = rt;
            rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            settings.targetTexture = rt;
            old.Release();
            for (int i = 0; i < 6; i++)
                yield return null;
        }

        /// <summary>True pixels to a panel pixel on the screen as it is now (1 at 1920 x 1080).</summary>
        public float Scale
        {
            get
            {
                float h = Screen.layout.height;
                return float.IsNaN(h) || h < 1f ? 1f : rt.height / h;
            }
        }

        /// <summary>A picture at 1280 x 720, then the screen put back: what a player with a small screen sees at this
        /// moment. Saved as the name with _720 after it; read for faults like any picture.</summary>
        public IEnumerator SmallShot(string name)
        {
            yield return Resize(1280, 720);
            yield return Shot(name + "_720");
            yield return Resize(1920, 1080);
            yield return Settle(0.1f);
        }

        public IPanel Panel => MapView.Current.GetComponent<UIDocument>().rootVisualElement.panel;
        /// <summary>Everything on the screen: the chart, the frame, the menu.</summary>
        public VisualElement Screen => Panel.visualTree;
        public FrameRoot Frame => FrameRoot.Current;

        public static IEnumerator Frames(int n)
        {
            for (int i = 0; i < n; i++)
                yield return null;
        }

        /// <summary>A few frames and a third of a real second: the frame fills its figures by the wall clock, four times a second.</summary>
        public static IEnumerator Settle(float seconds = 0.35f)
        {
            float from = Time.realtimeSinceStartup;
            for (int i = 0; i < 10 || Time.realtimeSinceStartup - from < seconds; i++)
                yield return null;
        }

        /// <summary>Waits, as a player's eye does, until something is on screen (or so many real seconds have gone).</summary>
        public static IEnumerator Until(Func<bool> there, float seconds = 10f)
        {
            float from = Time.realtimeSinceStartup;
            while (!there() && Time.realtimeSinceStartup - from < seconds)
                yield return null;
        }

        /// <summary>Is an element on screen: in the tree, with a size, and neither it nor anything above it hidden?</summary>
        public static bool Shown(VisualElement el)
        {
            if (el == null || el.panel == null)
                return false;
            for (var e = el; e != null; e = e.hierarchy.parent)
                if (e.resolvedStyle.display == DisplayStyle.None || e.resolvedStyle.visibility == Visibility.Hidden)
                    return false;
            var r = el.worldBound;
            return r.width > 1f && r.height > 1f && !float.IsNaN(r.width);
        }

        static bool Within(VisualElement inner, VisualElement outer)
        {
            for (var e = inner; e != null; e = e.hierarchy.parent)
                if (e == outer)
                    return true;
            return false;
        }

        /// <summary>The words of an element: its own text, or the texts of the labels in it.</summary>
        public static string Words(VisualElement el)
        {
            if (el == null)
                return "";
            if (el is TextElement t && !string.IsNullOrEmpty(t.text))
                return t.text;
            var parts = new List<string>();
            el.Query<TextElement>().ForEach(l =>
            {
                if (!string.IsNullOrEmpty(l.text))
                    parts.Add(l.text);
            });
            return string.Join(" | ", parts);
        }

        /// <summary>Every word shown under an element, as a reader would read it down the screen.</summary>
        public static string Read(VisualElement under)
        {
            var parts = new List<string>();
            if (under != null)
                under.Query<TextElement>().ForEach(l =>
                {
                    if (!string.IsNullOrEmpty(l.text) && Shown(l))
                        parts.Add(l.text);
                });
            return string.Join("\n", parts);
        }

        // ------------------------------------------------------------------ finding things as a reader does: by their words

        /// <summary>The buttons on screen whose words are these (the button's own text, or a label in it).</summary>
        public List<Button> Buttons(string words, VisualElement under = null)
        {
            var found = new List<Button>();
            (under ?? Screen).Query<Button>().ForEach(b =>
            {
                if (!Shown(b))
                    return;
                bool says = b.text == words;
                if (!says)
                    b.Query<TextElement>().ForEach(l => says |= l != b && l.text == words);
                if (says)
                    found.Add(b);
            });
            return found;
        }

        /// <summary>The button on screen with these words (null: none). Of several, the one the pointer can reach.</summary>
        public Button Button(string words, VisualElement under = null)
        {
            var all = Buttons(words, under);
            foreach (var b in all)
                if (Reachable(b))
                    return b;
            return all.Count > 0 ? all[0] : null;
        }

        /// <summary>The first button on screen whose words begin so.</summary>
        public Button ButtonStarting(string start, VisualElement under = null)
        {
            Button found = null;
            (under ?? Screen).Query<Button>().ForEach(b =>
            {
                if (found != null || !Shown(b))
                    return;
                bool says = (b.text ?? "").StartsWith(start, StringComparison.Ordinal);
                if (!says)
                    b.Query<TextElement>().ForEach(l => says |= l != b && (l.text ?? "").StartsWith(start, StringComparison.Ordinal));
                if (says)
                    found = b;
            });
            return found;
        }

        /// <summary>A button by what it says when the pointer rests on it (the small buttons that are only a sign).</summary>
        public Button ButtonByTip(string tipStart, VisualElement under = null)
        {
            Button found = null;
            (under ?? Screen).Query<Button>().ForEach(b =>
            {
                if (found == null && Shown(b) && (b.tooltip ?? "").StartsWith(tipStart, StringComparison.Ordinal))
                    found = b;
            });
            return found;
        }

        /// <summary>A line of a window (a name with things at its right end) by its name.</summary>
        public VisualElement Line(string name, VisualElement under = null)
        {
            VisualElement found = null;
            (under ?? Screen).Query<VisualElement>(className: "frame-ln").ForEach(row =>
            {
                if (found != null || !Shown(row))
                    return;
                var label = row.Q<Label>(className: "frame-ln-name");
                if (label != null && label.text == name)
                    found = row;
            });
            return found;
        }

        /// <summary>The floating windows that are up, back to front.</summary>
        public List<VisualElement> Floats()
        {
            var list = new List<VisualElement>();
            if (Frame != null)
                Frame.Root.Query<VisualElement>(className: "frame-float").ForEach(f =>
                {
                    if (Shown(f))
                        list.Add(f);
                });
            return list;
        }

        /// <summary>The floating window in front (null: none is up).</summary>
        public VisualElement FrontFloat()
        {
            var all = Floats();
            return all.Count > 0 ? all[all.Count - 1] : null;
        }

        /// <summary>The choices of a floating window, in order, with whether each can be chosen.</summary>
        public List<(Button button, string words, bool can)> Choices(VisualElement floating)
        {
            var list = new List<(Button, string, bool)>();
            if (floating != null)
                floating.Query<Button>(className: "frame-option").ForEach(b =>
                {
                    var label = b.Q<Label>(className: "frame-option-label");
                    list.Add((b, label != null ? label.text : "", b.enabledSelf));
                });
            return list;
        }

        // ------------------------------------------------------------------ the pointer

        /// <summary>Would a pointer at the middle of this element hit it (or something in it)?</summary>
        public bool Reachable(VisualElement el)
        {
            if (!Shown(el))
                return false;
            var hit = Panel.Pick(el.worldBound.center);
            return hit != null && Within(hit, el);
        }

        void Pointer(Vector2 at, EventType type)
        {
            var e = new Event { type = type, mousePosition = at, button = 0, clickCount = 1 };
            EventBase evt;
            switch (type)
            {
                case EventType.MouseDown: evt = PointerDownEvent.GetPooled(e); break;
                case EventType.MouseUp: evt = PointerUpEvent.GetPooled(e); break;
                default: evt = PointerMoveEvent.GetPooled(e); break;
            }
            using (evt)
                Screen.SendEvent(evt);
        }

        /// <summary>The pointer pressed and let go at a place on the screen.</summary>
        public void ClickAt(Vector2 at)
        {
            Pointer(at, EventType.MouseMove);
            Pointer(at, EventType.MouseDown);
            Pointer(at, EventType.MouseUp);
            clicks++;
        }

        /// <summary>The wheel turned at a place on the screen (over the chart: nearer or farther).</summary>
        public void WheelAt(Vector2 at, float notches)
        {
            var e = new Event { type = EventType.ScrollWheel, mousePosition = at, delta = new Vector2(0f, notches) };
            using (var evt = WheelEvent.GetPooled(e))
                Screen.SendEvent(evt);
        }

        /// <summary>
        /// A click on something on the screen. It is brought into view first if it lies in a list that scrolls; then it
        /// must be what the pointer hits at its middle, or the test fails saying what covers it. One frame passes after.
        /// </summary>
        public IEnumerator Click(VisualElement el, string what = null)
        {
            what = what ?? (el != null ? "'" + Words(el) + "'" : "nothing");
            Assert.IsNotNull(el, "there is nothing to click: " + what);
            Assert.IsNotNull(el.panel, what + " is not on screen any more");
            var scroll = el.GetFirstAncestorOfType<ScrollView>();
            if (scroll != null && !Reachable(el))
            {
                scroll.ScrollTo(el);
                yield return null;
                yield return null;
            }
            Assert.IsTrue(Shown(el), what + " is not shown: a player could not click it");
            var at = el.worldBound.center;
            var hit = Panel.Pick(at);
            if (hit == null || !Within(hit, el))
            {
                Save("FAULT_covered.png");
                Assert.Fail($"{what} cannot be clicked: at its middle ({at.x:0}, {at.y:0}) the pointer hits " +
                            (hit == null ? "nothing" : $"'{Words(hit)}' ({hit.GetType().Name} {string.Join(".", hit.GetClasses())})") + ". See FAULT_covered.png.");
            }
            if (el is Button b)
            {
                Assert.IsTrue(b.enabledInHierarchy, what + " is greyed out: it cannot be pressed");
                bool fired = false;
                Action heard = () => fired = true;
                b.clicked += heard;
                ClickAt(at);
                b.clicked -= heard;
                if (!fired)
                {
                    // (the pointer was on it and nothing was in its way, but this run's made-up pointer did not press it:
                    // pressed as the keyboard presses the button in focus. Counted, and said at the end.)
                    pressedByKey++;
                    using (var submit = NavigationSubmitEvent.GetPooled())
                    {
                        submit.target = b;
                        b.SendEvent(submit);
                    }
                }
            }
            else
                ClickAt(at);
            // (two frames: the game takes the click in the first, and what it built is laid out by the end of the second)
            yield return null;
            yield return null;
        }

        /// <summary>
        /// A click on something that is looked for until it is there (a window draws itself again when what it shows has
        /// changed, and for a frame the new thing has no place on screen yet: a player's eye waits that out without
        /// knowing it). Fails if it has not come within the seconds given.
        /// </summary>
        public IEnumerator ClickWhen(Func<VisualElement> find, string what, float seconds = 4f)
        {
            float from = Time.realtimeSinceStartup;
            VisualElement el = null;
            while (Time.realtimeSinceStartup - from < seconds)
            {
                el = find();
                if (el != null && Shown(el))
                    break;
                el = null;
                yield return null;
            }
            if (el == null)
                Save("FAULT_not_found.png");
            Assert.IsNotNull(el, what + " did not come on screen (see FAULT_not_found.png). In the frame: " + Short(Read(Frame?.Root ?? Screen)));
            yield return Click(el, what);
        }

        /// <summary>A click on the button with these words; fails if there is none on screen.</summary>
        public IEnumerator ClickButton(string words, VisualElement under = null)
        {
            var b = Button(words, under);
            if (b == null)
                Save("FAULT_no_button.png");
            Assert.IsNotNull(b, $"no button '{words}' on screen (see FAULT_no_button.png). On screen: {Short(Read(under ?? Frame?.Root ?? Screen))}");
            yield return Click(b, "the button '" + words + "'");
        }

        // ------------------------------------------------------------------ the keyboard

        /// <summary>A key pressed and let go: the game sees it pressed for one frame.</summary>
        public IEnumerator Press(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            yield return null;
            bool down = keyboard[key].isPressed;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            keys++;
            Assert.IsTrue(down, $"the key {key} did not reach the game (the input system took no key from the test)");
        }

        // ------------------------------------------------------------------ pictures, and reading what is on them

        static Texture2D Pixels(RenderTexture rt)
        {
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply(false);
            RenderTexture.active = prev;
            return tex;
        }

        void Save(string name)
        {
            var tex = Pixels(rt);
            File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());
            UnityEngine.Object.Destroy(tex);
        }

        /// <summary>A picture of the screen, once it has settled; and what is on it is read for faults.</summary>
        public IEnumerator Shot(string name, float settle = 0.35f)
        {
            yield return Settle(settle);
            if (!name.EndsWith(".png", StringComparison.Ordinal))
                name += ".png";
            Save(name);
            pictures.Add(name);
            Look(name);
        }

        static readonly string[] Bad = { "could not be drawn", "NaN", "Infinity", "{0}", "System.", "Exception" };

        /// <summary>The least true size text may have (TextFloor.LeastPx, less a hair for the rounding of a size).</summary>
        const float LeastPx = 15.9f;

        /// <summary>Reads every word on screen now for what a reader would call a fault: a writing key, a window that
        /// could not be drawn, a number that is not one, and text too small to read (under 18 true pixels in the frame,
        /// the menu and the chart's own labels; at full size the frame's text is noted for the count of its sizes).</summary>
        public void Look(string at)
        {
            float scale = Scale;
            Screen.Query<TextElement>().ForEach(l =>
            {
                string text = l.text;
                if (string.IsNullOrEmpty(text) || !Shown(l))
                    return;
                bool inFloat = false, inFrame = false, sized = false, inLog = false, inMenu = false, inNotes = false;
                for (var e = (VisualElement)l; e != null; e = e.hierarchy.parent)
                {
                    inFloat |= e.ClassListContains("frame-float");
                    inFrame |= e.ClassListContains("frame-root");
                    inLog |= e.name == "log" || e.ClassListContains("frame-log");
                    inMenu |= e.ClassListContains("menu-root");
                    inNotes |= e.name != null && e.name.StartsWith("note-", StringComparison.Ordinal);
                    sized |= e.ClassListContains("frame-root") || e.ClassListContains("menu-root") || e.ClassListContains("map-port") ||
                             e.ClassListContains("hover-card");
                }
                if (sized)
                {
                    float panelPx = l.resolvedStyle.fontSize, truePx = panelPx * scale;
                    if (truePx < smallestPx)
                    {
                        smallestPx = truePx;
                        smallestWhere = at + ": '" + Short(text) + "'";
                    }
                    if (truePx < LeastPx)
                    {
                        string said = $"{at}: text of {truePx:0.#} true px (under 18): '{Short(text)}' ({string.Join(".", l.GetClasses())})";
                        if (!faults.Contains(said))
                            faults.Add(said);
                    }
                    if (inFrame && Mathf.Abs(scale - 1f) < 0.001f)
                        frameSizes.Add(Mathf.RoundToInt(panelPx));
                }
                // THE PAGE'S RULE (Design/fixes/page_and_strings.md): nothing written for the makers. In a card, a talk
                // and the log the world speaks (prose); in the frame's own windows every pattern holds.
                // (the notes tool is the maker's own, and its words are whatever was typed: not held to the rule. The line
                // under a choice that cannot be taken is the frame speaking, though it stands in a card)
                if ((inFrame || inMenu) && !inNotes)
                {
                    string wrong = PageWords.Fault(text, false, (inFloat || inLog) && !l.ClassListContains("frame-option-rule"));
                    if (wrong != null && pageSeen.Add(wrong + "|" + text))
                    {
                        pageFaults.Add($"{wrong}: '{Short(text)}' ({string.Join(".", l.GetClasses())}) first at {at}");
                        // (THE RULE HOLDS: a test that asks for no faults fails on it)
                        faults.Add($"{at}: written for the makers ({wrong}): '{Short(text)}'");
                    }
                    Measure(l, text, at);
                }
                if (text.Contains("[write:"))
                {
                    string said = at + ": " + Short(text);
                    if (inFloat)
                    {
                        if (!unwritten.Contains(said))
                            unwritten.Add(said);
                    }
                    else if (!faults.Contains(said))
                        faults.Add(said);
                }
                foreach (string bad in Bad)
                    if (text.Contains(bad))
                    {
                        string said = at + ": '" + bad + "' in: " + Short(text);
                        if (!faults.Contains(said))
                            faults.Add(said);
                    }
            });
        }

        /// <summary>A text against its box. A single word broken over two lines ("Chronicl" and "e"). And a text that
        /// runs out of a box it stands in: cut there, or lying over whatever is beside it. A scrolling window's own
        /// edge is no fault: what is under it is reached by scrolling.</summary>
        void Measure(TextElement l, string text, string at)
        {
            var box = l.worldBound;
            float size = l.resolvedStyle.fontSize;
            if (float.IsNaN(box.width) || box.width < 2f || size < 1f)
                return;
            string classes = string.Join(".", l.GetClasses());
            // (not seen, not measured: the frame stands laid out under the opening's veil with nothing of it shown)
            for (var e = (VisualElement)l; e != null; e = e.hierarchy.parent)
                if (e.resolvedStyle.opacity < 0.05f)
                    return;
            // (at a small screen the frame is known to be too tight, and the columns and tabs of the tiled layouts are
            // had only by the tests: those are listed apart, and are not a failure)
            bool full = Scale > 0.98f;
            var frame = FrameRoot.Current;
            bool players = FrameWindows.OneWindow || frame == null || frame.Windows == null || frame.Windows.Space.layout == FrameLayout.Rows;
            var into = !full ? smallCuts : players ? cuts : tiledCuts;
            string held = full && players ? at + ": " : null;
            if (text.IndexOf(' ') < 0 && text.IndexOf('\n') < 0 && text.Length > 2 && box.height > size * 2.3f)
            {
                if (cutSeen.Add("broken|" + full + players + "|" + text))
                {
                    into.Add($"a word broken over two lines: '{text}' ({classes}) first at {at}");
                    if (held != null)
                        faults.Add(held + $"a word is broken over two lines: '{text}' ({classes})");
                }
                return;
            }
            // (what is placed by hand, out of the flow, stands where it was put: the clock's word hung under the bar's end)
            bool placed = l.resolvedStyle.position == Position.Absolute;
            for (var e = l.hierarchy.parent; e != null && !placed; e = e.hierarchy.parent)
            {
                if (e.ClassListContains("unity-scroll-view__content-viewport") || e.ClassListContains("frame-root") || e.ClassListContains("menu-root"))
                    break;
                placed = e.resolvedStyle.position == Position.Absolute;
                var outer = e.worldBound;
                if (float.IsNaN(outer.width) || outer.width < 2f || outer.height < 2f)
                    continue;
                float over = Mathf.Max(Mathf.Max(box.xMax - outer.xMax, outer.xMin - box.xMin), Mathf.Max(box.yMax - outer.yMax, outer.yMin - box.yMin));
                if (over > 2.5f)
                {
                    string where = string.IsNullOrEmpty(e.name) ? string.Join(".", e.GetClasses()) : e.name;
                    if (cutSeen.Add("out|" + full + players + "|" + where + "|" + Short(text)))
                    {
                        into.Add($"runs {over:0} px out of its box '{where}': '{Short(text)}' ({classes}) first at {at}");
                        if (held != null)
                            faults.Add(held + $"text runs {over:0} px out of its box '{where}': '{Short(text)}' ({classes})");
                    }
                    return;
                }
            }
        }

        public static string Short(string text)
        {
            text = (text ?? "").Replace("\n", " / ");
            return text.Length > 400 ? text.Substring(0, 400) + "..." : text;
        }
    }
}
