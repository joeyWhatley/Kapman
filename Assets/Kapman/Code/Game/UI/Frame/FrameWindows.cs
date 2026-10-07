using System;
using System.Collections.Generic;
using System.Text;

namespace Kapman.Game.Frame
{
    /// <summary>How a workspace lays its windows out: one above the other, side by side, or one at a time under tabs.</summary>
    public enum FrameLayout { Rows, Columns, Tabs }

    /// <summary>One workspace: its name, its way of laying out, the panels its windows hold (by id) and the one in focus.</summary>
    public sealed class FrameWorkspace
    {
        public string name = "";
        public FrameLayout layout = FrameLayout.Rows;
        public readonly List<string> slots = new List<string>();
        public int focus;
        /// <summary>How big each panel's window is in this workspace (FrameWindows.Small, Medium, Large), by the panel's
        /// id, so a window keeps its size when the windows are added to, closed or change places. Not named: medium.</summary>
        public readonly Dictionary<string, int> sizes = new Dictionary<string, int>();
    }

    /// <summary>
    /// THE WINDOWS OF THE FRAME (the user, 7 Oct 2026: the left side is "a modular window system like i3", "or sway",
    /// with "modular panels ... changeable with other types of panels"; Design/ui_mock/template3.html, layout G). A
    /// tiling window manager's state and nothing else, so it is plain C# the tests can drive: four named workspaces
    /// (keys 1 to 4), each keeping its own windows, its own layout (rows R, columns C, tabs T) and its own focus; a new
    /// window (N) takes the first panel the workspace does not show yet; close (X) leaves at least one; a window's title
    /// steps to the next panel not shown beside it, or picks any (one already shown changes places with it). The view
    /// (FrameRoot) draws it again whenever <see cref="Version"/> moves.
    /// </summary>
    public sealed class FrameWindows
    {
        /// <summary>The most windows a workspace holds.</summary>
        public const int MaxWindows = 4;

        public readonly List<FrameWorkspace> spaces = new List<FrameWorkspace>();
        /// <summary>The workspace on screen.</summary>
        public int Current { get; private set; }
        /// <summary>Counts up on every change.</summary>
        public int Version { get; private set; }
        /// <summary>The panels there are, in order (the registry's ids): what a new window, a step or a pick may show.</summary>
        public Func<IReadOnlyList<string>> panels = () => new string[0];

        public FrameWorkspace Space => spaces[Current];
        /// <summary>The panel in the focused window of the workspace on screen ("" if it has none).</summary>
        public string Focused => Space.slots.Count > 0 ? Space.slots[Space.focus] : "";

        /// <summary>The four workspaces a new player starts with (name, layout, panels). Panels not registered are left out
        /// when the windows are made, so a workspace may name a panel a later builder adds.</summary>
        public static readonly (string name, FrameLayout layout, string[] panels)[] Defaults =
        {
            ("Sea", FrameLayout.Rows, new[] { "voyage", "hold" }),
            ("Port", FrameLayout.Rows, new[] { "trade", "port", "people" }),
            // (7 Oct, later: the third is the mockup's Self, now that its panels are built; the word and the chronicle go to House)
            ("Self", FrameLayout.Tabs, new[] { "self", "bonds", "faith", "tongues" }),
            ("House", FrameLayout.Tabs, new[] { "house", "tasks", "word", "chronicle" }),
        };

        /// <summary>The index of the workspaces a new player starts with: at sea, and in port.</summary>
        public const int SeaSpace = 0, PortSpace = 1;

        /// <summary>
        /// ONE WINDOW ON THE SIDE (the user, 7 Oct 2026, night, after playing: "the text is far too crowded on the side so
        /// maybe just one panel"). The side holds one window, and the buttons over it say which. The workspaces are
        /// still there underneath (at sea she comes up on Voyage, in port on Port, each remembering the window it had
        /// last), but there is no bar of them, nothing is tiled, and no second window is opened. A test of the tiled
        /// windows sets this false before the frame is built.
        /// </summary>
        public static bool OneWindow = true;
        /// <summary>The one window each workspace starts with, in the order of <see cref="Defaults"/>.</summary>
        static readonly string[] OneDefaults = { "voyage", "port", "self", "house" };

        public FrameWindows(Func<IReadOnlyList<string>> panels)
        {
            if (panels != null)
                this.panels = panels;
            Reset();
        }

        /// <summary>Back to the workspaces a new player starts with.</summary>
        public void Reset()
        {
            spaces.Clear();
            var known = panels();
            for (int d = 0; d < Defaults.Length; d++)
            {
                var (name, layout, ids) = Defaults[d];
                var w = new FrameWorkspace { name = name, layout = OneWindow ? FrameLayout.Rows : layout };
                if (OneWindow)
                {
                    if (d < OneDefaults.Length && Has(known, OneDefaults[d]))
                        w.slots.Add(OneDefaults[d]);
                }
                else
                    foreach (string id in ids)
                        if (Has(known, id) && !w.slots.Contains(id) && w.slots.Count < MaxWindows)
                            w.slots.Add(id);
                spaces.Add(w);
            }
            foreach (var w in spaces)
                Mend(w, known);
            Current = 0;
            Version++;
        }

        static bool Has(IReadOnlyList<string> list, string id)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] == id)
                    return true;
            return false;
        }

        /// <summary>A workspace always has a window (if there is any panel at all), holds no panel twice and none that is
        /// not registered, and its focus is on one of them.</summary>
        static void Mend(FrameWorkspace w, IReadOnlyList<string> known)
        {
            for (int i = w.slots.Count - 1; i >= 0; i--)
                if (!Has(known, w.slots[i]) || w.slots.IndexOf(w.slots[i]) != i)
                    w.slots.RemoveAt(i);
            while (w.slots.Count > MaxWindows)
                w.slots.RemoveAt(w.slots.Count - 1);
            if (w.slots.Count == 0 && known.Count > 0)
                w.slots.Add(known[0]);
            w.focus = Math.Max(0, Math.Min(w.focus, w.slots.Count - 1));
        }

        // ------------------------------------------------------------------ what the keys and the clicks do

        /// <summary>Shows workspace `index` (0 to 3). False if it is the one on screen, or there is none such.</summary>
        public bool Go(int index)
        {
            if (index < 0 || index >= spaces.Count || index == Current)
                return false;
            Current = index;
            Version++;
            return true;
        }

        public void SetLayout(FrameLayout layout)
        {
            if (Space.layout == layout)
                return;
            Space.layout = layout;
            Version++;
        }

        /// <summary>The three sizes of a window (the ruling of 7 October: a panel's brief face is small, medium or large).</summary>
        public const int Small = 0, Medium = 1, Large = 2;

        /// <summary>The size of window `index` of the workspace on screen.</summary>
        public int SizeOf(int index)
        {
            var w = Space;
            return index >= 0 && index < w.slots.Count && w.sizes.TryGetValue(w.slots[index], out int s) ? s : Medium;
        }

        /// <summary>Sets the size of window `index` (kept with the arrangement, in the settings and in a saved game).</summary>
        public void SetSize(int index, int size)
        {
            var w = Space;
            if (index < 0 || index >= w.slots.Count)
                return;
            size = Math.Max(Small, Math.Min(Large, size));
            if (SizeOf(index) == size)
                return;
            if (size == Medium)
                w.sizes.Remove(w.slots[index]);
            else
                w.sizes[w.slots[index]] = size;
            Version++;
        }

        /// <summary>A new window, holding the first panel the workspace does not show yet; it takes the focus. False when
        /// the workspace is full or shows every panel.</summary>
        public bool Add()
        {
            if (OneWindow)
                return false;
            var w = Space;
            if (w.slots.Count >= MaxWindows)
                return false;
            foreach (string id in panels())
                if (!w.slots.Contains(id))
                {
                    w.slots.Add(id);
                    w.focus = w.slots.Count - 1;
                    Version++;
                    return true;
                }
            return false;
        }

        /// <summary>Closes window `index`; the last one stays. The focus keeps to the window it was on, or its neighbour.</summary>
        public bool Close(int index)
        {
            var w = Space;
            if (w.slots.Count <= 1 || index < 0 || index >= w.slots.Count)
                return false;
            w.slots.RemoveAt(index);
            if (w.focus > index || w.focus >= w.slots.Count)
                w.focus = Math.Max(0, w.focus - 1);
            Version++;
            return true;
        }

        /// <summary>The focus to window `index` (round the ends: one past the last is the first).</summary>
        public void Focus(int index)
        {
            var w = Space;
            if (w.slots.Count == 0)
                return;
            index = ((index % w.slots.Count) + w.slots.Count) % w.slots.Count;
            if (index == w.focus)
                return;
            w.focus = index;
            Version++;
        }

        /// <summary>The focus one window on (+1) or back (-1).</summary>
        public void FocusBy(int step) => Focus(Space.focus + step);

        /// <summary>Window `index` shows the next panel (+1) or the one before (-1) that the workspace does not show already.</summary>
        public bool Step(int index, int dir)
        {
            var w = Space;
            var all = panels();
            if (index < 0 || index >= w.slots.Count || all.Count == 0)
                return false;
            int at = 0;
            for (int i = 0; i < all.Count; i++)
                if (all[i] == w.slots[index])
                    at = i;
            for (int tries = 0; tries < all.Count; tries++)
            {
                at = ((at + (dir < 0 ? -1 : 1)) % all.Count + all.Count) % all.Count;
                if (!w.slots.Contains(all[at]))
                {
                    w.slots[index] = all[at];
                    Version++;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Window `index` shows panel `id`. If another window of the workspace shows it, the two change places.</summary>
        public bool Set(int index, string id)
        {
            var w = Space;
            if (index < 0 || index >= w.slots.Count || !Has(panels(), id) || w.slots[index] == id)
                return false;
            int other = w.slots.IndexOf(id);
            if (other >= 0)
                w.slots[other] = w.slots[index];
            w.slots[index] = id;
            Version++;
            return true;
        }

        /// <summary>Brings a panel up: the workspace on screen if it shows it (the focus goes to it), else the first
        /// workspace that does; else the focused window of the one on screen takes it. So a button anywhere can say
        /// "show the Voyage window".</summary>
        public void Show(string id)
        {
            if (!Has(panels(), id))
                return;
            if (OneWindow)
            {
                // (one window: it turns to the panel asked for, where she is)
                if (Space.slots.Count > 0)
                    Set(Space.focus, id);
                return;
            }
            int here = Space.slots.IndexOf(id);
            if (here >= 0)
            {
                Focus(here);
                return;
            }
            for (int s = 0; s < spaces.Count; s++)
            {
                int at = spaces[s].slots.IndexOf(id);
                if (at < 0)
                    continue;
                Current = s;
                spaces[s].focus = at;
                Version++;
                return;
            }
            Set(Space.focus, id);
        }

        // ------------------------------------------------------------------ kept between runs

        /// <summary>The whole arrangement as one line: "current;name|layout|focus|a,b,c;..." (the player's own, kept in
        /// the settings between runs).</summary>
        public string Capture()
        {
            var b = new StringBuilder();
            b.Append(Current);
            foreach (var w in spaces)
            {
                b.Append(';').Append(w.name).Append('|').Append((int)w.layout).Append('|').Append(w.focus).Append('|').Append(string.Join(",", w.slots));
                // (a fifth field, only when a window is not of the middle size: "panel:size,panel:size", in the windows' order)
                bool any = false;
                foreach (string id in w.slots)
                {
                    if (!w.sizes.TryGetValue(id, out int size) || size == Medium)
                        continue;
                    b.Append(any ? ',' : '|').Append(id).Append(':').Append(size);
                    any = true;
                }
            }
            return b.ToString();
        }

        /// <summary>The arrangement as kept. Anything it does not understand leaves the windows as they are; a panel that
        /// is no longer registered is dropped, and a workspace left empty gets the first panel.</summary>
        public bool Restore(string kept)
        {
            if (string.IsNullOrEmpty(kept))
                return false;
            string[] parts = kept.Split(';');
            if (parts.Length != spaces.Count + 1 || !int.TryParse(parts[0], out int current))
                return false;
            var read = new List<FrameWorkspace>();
            for (int i = 1; i < parts.Length; i++)
            {
                string[] f = parts[i].Split('|');
                // (four fields as kept before the sizes; a fifth with them)
                if ((f.Length != 4 && f.Length != 5) || !int.TryParse(f[1], out int layout) || !int.TryParse(f[2], out int focus) || layout < 0 || layout > 2)
                    return false;
                var w = new FrameWorkspace { name = spaces[i - 1].name, layout = (FrameLayout)layout, focus = focus };
                foreach (string id in f[3].Split(','))
                    if (id.Length > 0)
                        w.slots.Add(id);
                if (f.Length == 5)
                    foreach (string pair in f[4].Split(','))
                    {
                        int colon = pair.LastIndexOf(':');
                        if (colon > 0 && int.TryParse(pair.Substring(colon + 1), out int size) && size != Medium)
                            w.sizes[pair.Substring(0, colon)] = Math.Max(Small, Math.Min(Large, size));
                    }
                read.Add(w);
            }
            var known = panels();
            spaces.Clear();
            foreach (var w in read)
            {
                Mend(w, known);
                spaces.Add(w);
            }
            Current = Math.Max(0, Math.Min(current, spaces.Count - 1));
            Version++;
            if (OneWindow)
                foreach (var one in spaces)
                    if (one.slots.Count > 1)
                    {
                        string keep = one.slots[Math.Max(0, Math.Min(one.focus, one.slots.Count - 1))];
                        one.slots.Clear();
                        one.slots.Add(keep);
                        one.focus = 0;
                        one.layout = FrameLayout.Rows;
                    }
            return true;
        }
    }
}
