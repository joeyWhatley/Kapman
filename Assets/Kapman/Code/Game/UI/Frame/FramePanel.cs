using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// A PANEL OF THE FRAME (the user, 7 Oct 2026: "modular panels ... changeable with other types of panels", "at least
    /// 10 different panels"). One kind of window the left side can hold: Voyage, Trade, Hold...
    ///
    /// TO ADD ONE, write one file: a class that inherits this, with a public constructor that takes nothing. It is found
    /// by itself (FramePanels looks through the game's code for every such class), turns up in every window's list of
    /// panels, and can be named in a workspace's defaults (FrameWindows.Defaults). Give it:
    ///   <see cref="Id"/>     a short name that never changes ("crew"): the saved layout keeps it;
    ///   <see cref="Title"/>  what the window's title says ("Crew");
    ///   <see cref="Order"/>  its place in the list (the first panels are 10, 20, 30...);
    ///   <see cref="Fill"/>   builds what it shows into the window's body, from what the game knows now (FrameUi has the
    ///                        lines, figures, tags and buttons in the frame's look);
    ///   <see cref="Stamp"/>  a number that changes when what it shows has changed (FrameUi.Mix folds things into one):
    ///                        the frame asks a few times a second and calls Fill again only when it has moved, so a
    ///                        click is never lost to a redraw. A panel with a figure that runs all the time (a clock)
    ///                        changes its label in <see cref="Live"/> instead.
    /// One object is made of each panel and lives as long as the play does, so it may keep what the player picked in it.
    /// </summary>
    public abstract class FramePanel
    {
        public abstract string Id { get; }
        public abstract string Title { get; }
        public virtual int Order => 100;

        /// <summary>The body of the window that shows it now (null while no window does).</summary>
        public VisualElement Body { get; private set; }
        public bool Shown => Body != null;
        /// <summary>The frame it is shown in (for the log, the floating windows, the chart).</summary>
        public FrameRoot Frame { get; internal set; }

        int stamp;
        bool filled;
        VisualElement layoutRoot;                                     // the copy of the panel's own file in the body, if it has one

        /// <summary>
        /// THE PANEL'S OWN LAYOUT (KAPMAN_PLAN step 2d, 14 Oct 2026): a file Game/UI/Panels/&lt;Id&gt;.uxml, named in
        /// Resources/FrameParts, whose element "panel" is copied into the window's body before Fill; Fill puts its
        /// lines into the file's named sections through this. A panel without a file, or a section gone from it, gets
        /// its lines in the body itself as before. So the sections of a window can be moved, reordered or restyled in
        /// UI Builder without touching the code. A section is an element with the class frame-section (or a child of
        /// "panel") with the section's name.
        /// </summary>
        protected VisualElement Section(string name)
        {
            if (layoutRoot == null)
                return Body;
            // (a section carries the class frame-section; the lines put in earlier have parts of their own named
            // "left" and "name", which a search by name alone would find first)
            var e = layoutRoot.Q<VisualElement>(name, "frame-section");
            if (e == null)
                foreach (var c in layoutRoot.Children())
                    if (c.name == name)
                        return c;
            return e ?? Body;
        }

        internal void Attach(VisualElement body)
        {
            Body = body;
            filled = false;
            OnShown();
            Redraw();
        }

        internal void Detach()
        {
            if (Body == null)
                return;
            OnHidden();
            Body = null;
        }

        internal void Tick()
        {
            if (Body == null)
                return;
            if (!filled || Stamp() != stamp)
                Redraw();
            else
                Live();
        }

        /// <summary>Builds the body again now (after a button of the panel changed something).</summary>
        public void Redraw()
        {
            if (Body == null)
                return;
            filled = true;
            Body.Clear();
            layoutRoot = null;
            var file = FrameParts.Panel(Id);
            if (file != null)
            {
                layoutRoot = file.Instantiate().Q<VisualElement>("panel");
                if (layoutRoot == null)
                    Debug.LogError("Kapman frame: the window file of the panel \"" + Id + "\" (" + file.name + ".uxml) has no element named \"panel\": its lines go in the body as before.");
                else
                {
                    layoutRoot.RemoveFromHierarchy();
                    Body.Add(layoutRoot);
                }
            }
            try
            {
                Fill(Body);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Body.Add(FrameUi.P("This window could not be drawn: " + e.Message));
            }
            stamp = Stamp();
        }

        /// <summary>Builds what the panel shows into the window's body (emptied before the call).</summary>
        protected abstract void Fill(VisualElement body);
        /// <summary>A number that changes when what the panel shows has changed.</summary>
        protected virtual int Stamp() => 0;
        /// <summary>Between redraws, a few times a second: a running figure changed in place.</summary>
        protected virtual void Live() { }
        /// <summary>A window has begun to show the panel (before the first Fill).</summary>
        protected virtual void OnShown() { }
        /// <summary>No window shows it any more.</summary>
        protected virtual void OnHidden() { }
        /// <summary>A saved game was loaded under the frame: what the panel kept of the game before (the words of the last
        /// deal, a port picked) is dropped. One object of a panel lives for the whole play, so it must be told.</summary>
        protected virtual void OnLoaded() { }

        internal void LoadedGame()
        {
            try
            {
                OnLoaded();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            filled = false;                                              // (drawn again at the next look)
        }
    }

    /// <summary>
    /// THE PANELS THERE ARE: every FramePanel class of the game's code, one object of each, in their order. A panel made
    /// by hand elsewhere (another assembly, a test) joins with <see cref="Register"/>.
    /// </summary>
    public static class FramePanels
    {
        static readonly List<FramePanel> all = new List<FramePanel>();
        static readonly List<string> ids = new List<string>();
        static bool found;

        public static IReadOnlyList<FramePanel> All
        {
            get
            {
                Find();
                return all;
            }
        }

        /// <summary>The panels' ids, in order (what the windows' state goes by).</summary>
        public static IReadOnlyList<string> Ids
        {
            get
            {
                Find();
                return ids;
            }
        }

        public static FramePanel Get(string id)
        {
            Find();
            foreach (var p in all)
                if (p.Id == id)
                    return p;
            return null;
        }

        public static string Title(string id) => Get(id)?.Title ?? id ?? "";

        /// <summary>Adds a panel made by hand (one with the same id is replaced).</summary>
        public static void Register(FramePanel panel)
        {
            if (panel == null || string.IsNullOrEmpty(panel.Id))
                return;
            Find();
            all.RemoveAll(p => p.Id == panel.Id);
            all.Add(panel);
            Sort();
        }

        static void Find()
        {
            if (found)
                return;
            found = true;
            foreach (var type in typeof(FramePanel).Assembly.GetTypes())
            {
                if (type.IsAbstract || !typeof(FramePanel).IsAssignableFrom(type) || type.GetConstructor(Type.EmptyTypes) == null)
                    continue;
                try
                {
                    var p = (FramePanel)Activator.CreateInstance(type);
                    if (!string.IsNullOrEmpty(p.Id) && !all.Exists(x => x.Id == p.Id))
                        all.Add(p);
                    else
                        Debug.LogWarning($"Kapman frame: the panel {type.Name} has no id, or one another panel has: left out.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Kapman frame: the panel {type.Name} could not be made: {e.Message}");
                }
            }
            Sort();
        }

        static void Sort()
        {
            all.Sort((a, b) => a.Order != b.Order ? a.Order.CompareTo(b.Order) : string.CompareOrdinal(a.Id, b.Id));
            ids.Clear();
            foreach (var p in all)
                ids.Add(p.Id);
        }

        // Enter Play Mode runs without a domain reload: each play makes its panels again (they keep what was picked)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            all.Clear();
            ids.Clear();
            found = false;
        }
    }

    /// <summary>
    /// THE FRAME'S PARTS, in its look (Assets/Kapman/Game/UI/Frame.uss): what a panel's Fill is built from. A heading, a
    /// plain paragraph, a line (a name with small print under it and things at its right end), a figure in gold (what is
    /// the player's to spend or count), a tag, a button, a row of pips. Nothing here knows the game.
    /// </summary>
    public static class FrameUi
    {
        public static Label H(string text) => Words(text, "frame-h");
        /// <summary>Small print: a note, a hint, what something means.</summary>
        public static Label P(string text) => Words(text, "frame-p");
        /// <summary>Reading text: what someone says, what happened.</summary>
        public static Label Text(string text) => Words(text, "frame-text");
        /// <summary>A figure that is the player's to spend or to count: gold.</summary>
        public static Label Num(string text) => Words(text, "frame-num");
        public static Label Tag(string text) => Words(text, "frame-tag");

        static Label Words(string text, string cls)
        {
            var l = new Label(text ?? "");
            l.AddToClassList(cls);
            return l;
        }

        /// <summary>A line: its name, small print under it (or none), and what stands at its right end (figures, tags, buttons).</summary>
        public static VisualElement Ln(string name, string small, params VisualElement[] right)
        {
            // (laid out in Game/UI/Parts/Line.uxml)
            var parts = FrameParts.Current;
            var row = FrameParts.Make<VisualElement>(parts != null ? parts.line : null, "line");
            FrameParts.In<Label>(row, "name").text = name ?? "";
            var print = FrameParts.In<Label>(row, "small");
            if (string.IsNullOrEmpty(small))
                print.RemoveFromHierarchy();
            else
                print.text = small;
            if (right != null)
                foreach (var r in right)
                    if (r != null)
                        row.Add(r);
            return row;
        }

        /// <summary>A line that is a button: clicking anywhere on it does the thing (a port to pick, a choice to make).</summary>
        public static Button LnButton(string name, string small, Action click, params VisualElement[] right)
        {
            // (laid out in Game/UI/Parts/LineButton.uxml)
            var parts = FrameParts.Current;
            var b = FrameParts.Make<Button>(parts != null ? parts.lineButton : null, "line");
            if (click != null)
                b.clicked += click;
            FrameParts.In<Label>(b, "name").text = name ?? "";
            var print = FrameParts.In<Label>(b, "small");
            if (string.IsNullOrEmpty(small))
                print.RemoveFromHierarchy();
            else
                print.text = small;
            if (right != null)
                foreach (var r in right)
                    if (r != null)
                    {
                        r.pickingMode = PickingMode.Ignore;
                        b.Add(r);
                    }
            return b;
        }

        /// <summary>A small button.</summary>
        public static Button Btn(string text, Action click, string tip = null)
        {
            var b = new Button(click) { text = text ?? "" };
            b.AddToClassList("frame-k");
            if (!string.IsNullOrEmpty(tip))
                b.tooltip = tip;
            return b;
        }

        /// <summary>The one button a panel is for ("Cast off for Lundenwic").</summary>
        public static Button Go(string text, Action click)
        {
            var b = Btn(text, click);
            b.AddToClassList("go");
            return b;
        }

        /// <summary>Buttons side by side, wrapping.</summary>
        public static VisualElement Row(params VisualElement[] items)
        {
            var row = new VisualElement();
            row.AddToClassList("frame-row");
            if (items != null)
                foreach (var i in items)
                    if (i != null)
                        row.Add(i);
            return row;
        }

        /// <summary>So many of five (or of `of`) as filled and hollow dots: a stat, a tongue, a bond.</summary>
        public static VisualElement Pips(int n, int of = 5)
        {
            var row = new VisualElement();
            row.AddToClassList("frame-pips");
            for (int i = 1; i <= of; i++)
            {
                var pip = new VisualElement();
                pip.AddToClassList("frame-pip");
                pip.EnableInClassList("off", i > n);
                row.Add(pip);
            }
            return row;
        }

        // ---- a stamp from many things (FramePanel.Stamp)
        public static int Mix(int h, int v) => unchecked(h * 31 + v);
        public static int Mix(int h, string s) => Mix(h, s == null ? 0 : StableHash(s));
        public static int Mix(int h, double v) => Mix(h, v.GetHashCode());
        public static int Mix(int h, bool v) => Mix(h, v ? 1 : 2);

        static int StableHash(string s)
        {
            unchecked
            {
                int h = 17;
                for (int i = 0; i < s.Length; i++)
                    h = h * 31 + s[i];
                return h;
            }
        }

        // ---- plain words for figures

        /// <summary>A stretch of days as a merchant says it, to the half day: "half a day", "1 day", "2 and a half days".</summary>
        public static string Days(double days)
        {
            double half = Math.Max(0.5, Math.Round(days * 2.0) / 2.0);
            if (half <= 0.5)
                return "half a day";
            int whole = (int)Math.Floor(half);
            bool and = half - whole > 0.25;
            if (whole == 1 && !and)
                return "1 day";
            return and ? $"{whole} and a half days" : $"{whole} days";
        }

        /// <summary>A low and a high figure of days: "1 to 2 days", or one figure where they come to the same.</summary>
        public static string Days(double low, double high)
        {
            double lo = Math.Max(0.5, Math.Round(Math.Min(low, high) * 2.0) / 2.0), hi = Math.Max(0.5, Math.Round(Math.Max(low, high) * 2.0) / 2.0);
            if (Math.Abs(hi - lo) < 0.25)
                return Days(lo);
            if (lo <= 0.5)
                return "half a day to " + Days(hi);
            return $"{Plain(lo)} to {Plain(hi)} days";
        }

        static string Plain(double halves)
        {
            int whole = (int)Math.Floor(halves);
            return halves - whole > 0.25 ? $"{whole} and a half" : whole.ToString();
        }

        /// <summary>How long ago, in days: "today", "yesterday", "6 days ago".</summary>
        public static string Ago(double days) => days < 1.0 ? "today" : days < 2.0 ? "yesterday" : $"{days:0} days ago";

        /// <summary>The part of the day an hour falls in, as it is said aboard (no clock strikes at sea).</summary>
        public static string PartOfDay(double hour)
        {
            hour = ((hour % 24.0) + 24.0) % 24.0;
            return hour < 5.0 ? "the small hours" : hour < 8.0 ? "early morning" : hour < 11.5 ? "morning"
                 : hour < 13.5 ? "midday" : hour < 17.0 ? "afternoon" : hour < 21.0 ? "evening" : "night";
        }

        /// <summary>The airt a compass bearing lies in: "north", "north-east"...</summary>
        public static string Airt(double degrees)
        {
            string[] names = { "north", "north-east", "east", "south-east", "south", "south-west", "west", "north-west" };
            int i = (int)Math.Round((((degrees % 360.0) + 360.0) % 360.0) / 45.0) % 8;
            return names[i];
        }
    }
}
