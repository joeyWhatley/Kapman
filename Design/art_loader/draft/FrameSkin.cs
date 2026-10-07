// PROJECT PATH: Assets/Kapman/Code/Game/UI/Frame/FrameSkin.cs
// (a draft of 7 Oct 2026, written outside Assets: Design/art_loader/PLAN.md says how to bring it in)
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE FRAME'S OWN PIECES, LAID OVER ITS ELEMENTS (the user, 7 Oct 2026: "for the UI and really everything I will need
    /// templates for and assigned correctly ... so I can make the sprites myself"). Art/hud_assign.json names every piece
    /// of the Command frame: its file, its size, how it is cut in nine, and (its "uss" field) which elements of the frame
    /// take it, written as the stylesheet writes them: ".frame-verb.on, .frame-verb:active".
    ///
    /// THE RULE: a piece is used only when the user has drawn it (it is in Art/final). Until then the element keeps the
    /// look Frame.uss gives it, so a set that is half drawn never looks broken. Where an element has a piece for the
    /// state it is in (a window in focus, a button that is on) and that piece is not drawn yet, the element keeps its
    /// stylesheet look in that state, even if the plain piece is drawn: the states stay told apart.
    ///
    /// HOW A PIECE IS LAID (its "lay" field):
    ///   fill  the element's background, cut in nine by the piece's "slice" (corners kept, sides stretched), each pixel
    ///         of the piece shown as <see cref="Scale"/> of the screen's; the stylesheet's own edge, corners and ground
    ///         colour stand down
    ///   over  a layer on top of the element (a hollow frame round a portrait or round the ship's view)
    ///   icon  the element shows the picture in place of its own drawing (FrameIcon and FrameGlyph ask
    ///         <see cref="Skinned"/> and draw nothing), at the biggest whole multiple of the picture's pixels that fits
    ///
    /// The frame builds and rebuilds its elements all the time, so the skin walks them: a few times a second
    /// (<see cref="Tick"/>), at once where the frame says something was just built (<see cref="Apply(VisualElement)"/>),
    /// and at once under the cursor (:hover, :active).
    /// </summary>
    public sealed class FrameSkin
    {
        public const string Fill = "fill", Over = "over", Icon = "icon";

        /// <summary>One piece of Art/hud_assign.json.</summary>
        public sealed class Piece
        {
            public string id = "", file = "", uss = "", lay = Fill;
            public int w, h;
            /// <summary>Left, top, right, bottom in the piece's own pixels; null: not cut.</summary>
            public int[] slice;
        }

        /// <summary>One pixel of a piece cut in nine is so many of the screen's (the pieces are drawn for a 640 x 360
        /// screen and the frame is laid out for 1920 x 1080).</summary>
        public float Scale = 3f;
        /// <summary>A place made for an icon (<see cref="Place"/>) is the icon's size times this.</summary>
        public float PlaceScale = 2f;

        /// <summary>The class an element has while a piece is laid on it in place of its own drawing.</summary>
        public const string SkinnedClass = "skinned";
        /// <summary>The class of a place made for an icon: it takes no room until its piece is drawn.</summary>
        public const string PlaceClass = "frame-art";
        const string On = "skin-on", MarkStart = "skin--", LayFill = "skin-lay-fill", LayOver = "skin-lay-over", LayIcon = "skin-lay-icon";
        const string OverName = "frame-skin-over", ScreenClass = "frame-screen";

        sealed class Part
        {
            public string[] classes;
            public bool hover, active;
        }

        sealed class Rule
        {
            public Part[] parts;
            public string last;                                           // the first class of the last part: a quick no
            public Piece piece;
            public int weight;
            public string mark, layMark;
            public Texture2D tex;                                         // the user's picture, or null: not drawn
        }

        readonly ArtSlots art;
        readonly List<Rule> rules = new List<Rule>();
        readonly List<string> skipped = new List<string>();
        readonly List<string> scratch = new List<string>();
        readonly HashSet<string> kept = new HashSet<string>();
        VisualElement root, screen, over, down;
        float next;
        bool stale;

        /// <summary>How many places the pieces have (selectors read from "uss").</summary>
        public int RuleCount => rules.Count;
        /// <summary>The selectors that could not be read (anything but classes, :hover and :active).</summary>
        public IReadOnlyList<string> Skipped => skipped;

        // ------------------------------------------------------------------ reading hud_assign.json

        /// <summary>The pieces of a hud_assign.json (its text). A file that does not parse gives none.</summary>
        public static List<Piece> Read(string json)
        {
            var list = new List<Piece>();
            object o;
            try
            {
                o = Json.Parse(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Kapman frame skin: hud_assign.json could not be read (" + e.Message + "): the frame keeps its stylesheet look.");
                return list;
            }
            var pieces = Json.Obj(o, "pieces");
            if (pieces == null)
                return list;
            foreach (var kv in pieces)
            {
                var p = new Piece
                {
                    id = kv.Key, file = Json.Str(kv.Value, "file"), uss = Json.Str(kv.Value, "uss"), lay = Json.Str(kv.Value, "lay", Fill),
                    w = (int)Json.Num(kv.Value, "w"), h = (int)Json.Num(kv.Value, "h"),
                };
                var cut = Json.Arr(kv.Value, "slice");
                if (cut != null && cut.Count == 4 && cut[0] is double l && cut[1] is double t && cut[2] is double r && cut[3] is double b)
                    p.slice = new[] { (int)l, (int)t, (int)r, (int)b };
                if (p.file.Length > 0)
                    list.Add(p);
            }
            return list;
        }

        /// <summary>The pieces of a hud_assign.json on disk (none when it is not there).</summary>
        public static List<Piece> ReadFile(string path)
        {
            try
            {
                return File.Exists(path) ? Read(File.ReadAllText(path)) : new List<Piece>();
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogWarning("Kapman frame skin: " + path + " could not be read (" + e.Message + ").");
                return new List<Piece>();
            }
        }

        /// <summary>The skin of the game's frame: the pieces of Art/hud_assign.json, the game's pictures, on this root.</summary>
        public static FrameSkin ForFrame(VisualElement root)
        {
            string path = Path.Combine(ArtSlots.Folder, "hud_assign.json");
            var skin = new FrameSkin(ArtSlots.Shared, ReadFile(path));
            if (skin.RuleCount == 0)
                Debug.Log("Kapman frame skin: " + path + " gives no piece a place (no \"uss\" fields): the frame keeps its stylesheet look.");
            foreach (string s in skin.Skipped)
                Debug.LogWarning("Kapman frame skin: the place \"" + s + "\" could not be read (classes, :hover and :active only).");
            skin.Attach(root);
            return skin;
        }

        public FrameSkin(ArtSlots art, IEnumerable<Piece> pieces)
        {
            this.art = art;
            if (pieces == null)
                return;
            foreach (var p in pieces)
            {
                if (p == null || string.IsNullOrWhiteSpace(p.uss))
                    continue;
                foreach (string one in p.uss.Split(','))
                {
                    string sel = one.Trim();
                    if (sel.Length == 0)
                        continue;
                    var parts = Parse(sel);
                    if (parts == null)
                    {
                        skipped.Add(sel);
                        continue;
                    }
                    int weight = parts.Length;
                    foreach (var part in parts)
                        weight += 10 * (part.classes.Length + (part.hover ? 1 : 0) + (part.active ? 1 : 0));
                    rules.Add(new Rule
                    {
                        parts = parts, last = parts[parts.Length - 1].classes[0], piece = p, weight = weight,
                        mark = MarkStart + p.id.Replace('.', '-'), layMark = p.lay == Over ? LayOver : p.lay == Icon ? LayIcon : LayFill,
                    });
                }
            }
            Pictures();
        }

        /// <summary>".frame-verb.on:active .frame-icon" as its parts; null when it holds anything but classes, :hover and
        /// :active (every part must name a class).</summary>
        static Part[] Parse(string selector)
        {
            var parts = new List<Part>();
            foreach (string word in selector.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var classes = new List<string>();
                var part = new Part();
                int i = 0;
                while (i < word.Length)
                {
                    char kind = word[i];
                    if (kind != '.' && kind != ':')
                        return null;
                    int j = i + 1;
                    while (j < word.Length && word[j] != '.' && word[j] != ':')
                        j++;
                    string name = word.Substring(i + 1, j - i - 1);
                    if (name.Length == 0)
                        return null;
                    if (kind == '.')
                        classes.Add(name);
                    else if (name == "hover")
                        part.hover = true;
                    else if (name == "active")
                        part.active = true;
                    else
                        return null;
                    i = j;
                }
                if (classes.Count == 0)
                    return null;
                part.classes = classes.ToArray();
                parts.Add(part);
            }
            return parts.Count > 0 ? parts.ToArray() : null;
        }

        /// <summary>Which pieces are drawn: each rule is given the user's picture, or none. The pictures are kept for as
        /// long as the skin is.</summary>
        void Pictures()
        {
            foreach (var r in rules)
            {
                if (kept.Add(r.piece.file))
                    art.Keep(r.piece.file, true);
                r.tex = art.Final(r.piece.file);
            }
        }

        // ------------------------------------------------------------------ on the frame

        /// <summary>Takes a frame's root: the skin follows the cursor on it, makes the screen's own frame under its
        /// parts, and lays what is drawn.</summary>
        public void Attach(VisualElement frameRoot)
        {
            Detach();
            root = frameRoot;
            if (root == null)
                return;
            screen = new VisualElement { pickingMode = PickingMode.Ignore };
            screen.AddToClassList(ScreenClass);
            screen.style.position = Position.Absolute;
            screen.style.left = screen.style.top = screen.style.right = screen.style.bottom = 0f;
            root.Insert(0, screen);
            root.RegisterCallback<PointerOverEvent>(OnOver, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerOutEvent>(OnOut, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerDownEvent>(OnDown, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerUpEvent>(OnUp, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerCaptureOutEvent>(OnLost, TrickleDown.TrickleDown);
            art.Changed += OnArt;
            Apply();
        }

        /// <summary>Lets the frame go: every element has its stylesheet look again.</summary>
        public void Detach()
        {
            if (root == null)
                return;
            art.Changed -= OnArt;
            root.UnregisterCallback<PointerOverEvent>(OnOver, TrickleDown.TrickleDown);
            root.UnregisterCallback<PointerOutEvent>(OnOut, TrickleDown.TrickleDown);
            root.UnregisterCallback<PointerDownEvent>(OnDown, TrickleDown.TrickleDown);
            root.UnregisterCallback<PointerUpEvent>(OnUp, TrickleDown.TrickleDown);
            root.UnregisterCallback<PointerCaptureOutEvent>(OnLost, TrickleDown.TrickleDown);
            StripAll(root);
            screen?.RemoveFromHierarchy();
            screen = null;
            root = null;
            over = down = null;
        }

        /// <summary>The skin is done with: its pictures are let go.</summary>
        public void Dispose()
        {
            Detach();
            foreach (string file in kept)
                art.Release(file, true);
            kept.Clear();
        }

        /// <summary>Each frame: lays the pieces a few times a second (the frame rebuilds its lines and windows as it
        /// goes), and at once when a picture on disk changed.</summary>
        public void Tick()
        {
            if (root == null)
                return;
            float now = Time.realtimeSinceStartup;
            if (!stale && now < next)
                return;
            next = now + 0.25f;
            Apply();
        }

        /// <summary>Lays the pieces on the whole frame now.</summary>
        public void Apply()
        {
            if (root == null)
                return;
            if (stale)
            {
                stale = false;
                StripAll(root);
                Pictures();
            }
            Walk(root);
        }

        /// <summary>Lays the pieces on one part of the frame that was just built (a window, a panel's body, a floating
        /// window), so it never shows a moment in the stylesheet's look.</summary>
        public void Apply(VisualElement under)
        {
            if (under != null && root != null && !stale)
                Walk(under);
        }

        void OnArt() => stale = true;                                     // (a piece drawn, changed or taken away: all is laid again)

        void Walk(VisualElement e)
        {
            if (e.name == OverName)
                return;
            Skin(e);
            var h = e.hierarchy;
            for (int i = 0; i < h.childCount; i++)
                Walk(h[i]);
        }

        void StripAll(VisualElement e)
        {
            if (e.ClassListContains(On))
                Strip(e);
            var h = e.hierarchy;
            for (int i = 0; i < h.childCount; i++)
                StripAll(h[i]);
        }

        // ------------------------------------------------------------------ which piece an element takes

        /// <summary>The piece an element takes as things stand (its classes, its ancestors', the cursor): the one whose
        /// place fits it most exactly, and null when that piece is not drawn yet (THE RULE) or no place fits.</summary>
        public Piece PieceFor(VisualElement e) => Best(e)?.piece;

        /// <summary>The same without THE RULE: the piece whose place fits, drawn or not (tests, and looking into it).</summary>
        public Piece PlaceFor(VisualElement e) => Fits(e)?.piece;

        Rule Best(VisualElement e)
        {
            var r = Fits(e);
            return r != null && r.tex != null ? r : null;
        }

        Rule Fits(VisualElement e)
        {
            Rule best = null;
            for (int i = 0; i < rules.Count; i++)
            {
                var r = rules[i];
                if (!e.ClassListContains(r.last) || (best != null && r.weight < best.weight))
                    continue;
                if (Matches(r, e))
                    best = r;                                              // (as exact, and later in the list: the later one)
            }
            return best;
        }

        bool Matches(Rule r, VisualElement e)
        {
            int last = r.parts.Length - 1;
            if (!Has(e, r.parts[last]))
                return false;
            var up = e.hierarchy.parent;
            for (int i = last - 1; i >= 0; i--)
            {
                while (up != null && !Has(up, r.parts[i]))
                    up = up.hierarchy.parent;
                if (up == null)
                    return false;
                up = up.hierarchy.parent;
            }
            return true;
        }

        bool Has(VisualElement e, Part p)
        {
            for (int i = 0; i < p.classes.Length; i++)
                if (!e.ClassListContains(p.classes[i]))
                    return false;
            if (p.hover && !Under(over, e))
                return false;
            return !p.active || Under(down, e);
        }

        static bool Under(VisualElement at, VisualElement e) => at != null && (at == e || e.Contains(at));

        // ------------------------------------------------------------------ laying a piece, and taking it off

        void Skin(VisualElement e)
        {
            var r = Best(e);
            if (r == null)
            {
                if (e.ClassListContains(On))
                    Strip(e);
                return;
            }
            if (!e.ClassListContains(r.mark))
            {
                if (e.ClassListContains(On))
                    Strip(e);
                Dress(e, r);
            }
            if (r.piece.lay == Icon)
                FitIcon(e, r);
            else if (r.piece.lay != Over)
                FitCuts(e, r);
        }

        /// <summary>The corners never take more room than the element has: each pixel of the piece is shown as
        /// <see cref="Scale"/> of the screen's, or as fewer (a whole number) on an element too small for that (a tag, a
        /// chip, the scroll bar's thumb as Frame.uss sizes them today).</summary>
        void FitCuts(VisualElement e, Rule r)
        {
            var cut = r.piece.slice;
            if (cut == null)
                return;
            float k = Scale, w = e.resolvedStyle.width, h = e.resolvedStyle.height;
            if (!float.IsNaN(w) && !float.IsNaN(h) && w > 0f && h > 0f)
            {
                if (cut[0] + cut[2] > 0 && (cut[0] + cut[2]) * k > w)
                    k = Mathf.Floor(w / (cut[0] + cut[2]));
                if (cut[1] + cut[3] > 0 && (cut[1] + cut[3]) * k > h)
                    k = Mathf.Floor(h / (cut[1] + cut[3]));
            }
            e.style.unitySliceScale = Mathf.Max(1f, k);
        }

        void Dress(VisualElement e, Rule r)
        {
            var s = e.style;
            e.AddToClassList(On);
            e.AddToClassList(r.mark);
            e.AddToClassList(r.layMark);
            if (r.piece.lay == Over)
            {
                // the element's own edge stands down, and the piece lies over all that is in it
                s.borderLeftWidth = s.borderTopWidth = s.borderRightWidth = s.borderBottomWidth = 0f;
                s.borderTopLeftRadius = s.borderTopRightRadius = s.borderBottomLeftRadius = s.borderBottomRightRadius = 0f;
                var layer = new VisualElement { name = OverName, pickingMode = PickingMode.Ignore };
                layer.style.position = Position.Absolute;
                layer.style.left = layer.style.top = layer.style.right = layer.style.bottom = 0f;
                Paint(layer.style, r);
                e.hierarchy.Add(layer);
            }
            else if (r.piece.lay == Icon)
            {
                s.backgroundImage = new StyleBackground(r.tex);
                s.backgroundRepeat = new StyleBackgroundRepeat(new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat));
                s.backgroundPositionX = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Center));
                s.backgroundPositionY = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Center));
                e.AddToClassList(SkinnedClass);
                e.MarkDirtyRepaint();                                     // (its own drawing stands down: Skinned)
                if (e.ClassListContains(PlaceClass))
                {
                    s.width = r.piece.w * PlaceScale;
                    s.height = r.piece.h * PlaceScale;
                    s.display = DisplayStyle.Flex;
                }
            }
            else
            {
                s.backgroundColor = Color.clear;
                s.borderLeftColor = s.borderTopColor = s.borderRightColor = s.borderBottomColor = Color.clear;
                s.borderTopLeftRadius = s.borderTopRightRadius = s.borderBottomLeftRadius = s.borderBottomRightRadius = 0f;
                Paint(s, r);
            }
        }

        void Paint(IStyle s, Rule r)
        {
            s.backgroundImage = new StyleBackground(r.tex);
            var cut = r.piece.slice;
            if (cut != null && cut[0] + cut[2] < r.tex.width && cut[1] + cut[3] < r.tex.height)
            {
                s.unitySliceLeft = cut[0];
                s.unitySliceTop = cut[1];
                s.unitySliceRight = cut[2];
                s.unitySliceBottom = cut[3];
                s.unitySliceScale = Scale;
            }
            else
                s.backgroundSize = new StyleBackgroundSize(new BackgroundSize(Length.Percent(100f), Length.Percent(100f)));
        }

        /// <summary>An icon is shown at the biggest whole multiple of its own pixels that fits the element (never a
        /// part of a pixel), and pale where the button it is on cannot be used.</summary>
        void FitIcon(VisualElement e, Rule r)
        {
            float w = e.resolvedStyle.width, h = e.resolvedStyle.height;
            float k = 1f;
            if (e.ClassListContains(PlaceClass))
                k = Mathf.Max(1f, Mathf.Floor(PlaceScale));                 // (a place was made the icon's size times this)
            else if (!float.IsNaN(w) && !float.IsNaN(h) && r.piece.w > 0 && r.piece.h > 0)
                k = Mathf.Max(1f, Mathf.Floor(Mathf.Min(w / r.piece.w, h / r.piece.h) + 0.001f));
            e.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(new Length(r.piece.w * k), new Length(r.piece.h * k)));
            bool off = false;
            var up = e.hierarchy.parent;
            for (int i = 0; i < 3 && up != null && !off; i++, up = up.hierarchy.parent)
                off = up.ClassListContains("off");
            e.style.unityBackgroundImageTintColor = off ? new StyleColor(new Color(1f, 1f, 1f, 0.35f)) : new StyleColor(StyleKeyword.Null);
        }

        void Strip(VisualElement e)
        {
            var s = e.style;
            if (e.ClassListContains(LayOver))
            {
                var h = e.hierarchy;
                for (int i = h.childCount - 1; i >= 0; i--)
                    if (h[i].name == OverName)
                        h.RemoveAt(i);
                s.borderLeftWidth = s.borderTopWidth = s.borderRightWidth = s.borderBottomWidth = StyleKeyword.Null;
                s.borderTopLeftRadius = s.borderTopRightRadius = s.borderBottomLeftRadius = s.borderBottomRightRadius = StyleKeyword.Null;
            }
            else if (e.ClassListContains(LayIcon))
            {
                s.backgroundImage = new StyleBackground(StyleKeyword.Null);
                s.backgroundSize = StyleKeyword.Null;
                s.backgroundRepeat = StyleKeyword.Null;
                s.backgroundPositionX = StyleKeyword.Null;
                s.backgroundPositionY = StyleKeyword.Null;
                s.unityBackgroundImageTintColor = StyleKeyword.Null;
                e.RemoveFromClassList(SkinnedClass);
                e.MarkDirtyRepaint();
                if (e.ClassListContains(PlaceClass))
                {
                    s.width = s.height = StyleKeyword.Null;
                    s.display = DisplayStyle.None;
                }
            }
            else
            {
                s.backgroundImage = new StyleBackground(StyleKeyword.Null);
                s.backgroundColor = StyleKeyword.Null;
                s.backgroundSize = StyleKeyword.Null;
                s.borderLeftColor = s.borderTopColor = s.borderRightColor = s.borderBottomColor = StyleKeyword.Null;
                s.borderTopLeftRadius = s.borderTopRightRadius = s.borderBottomLeftRadius = s.borderBottomRightRadius = StyleKeyword.Null;
                s.unitySliceLeft = s.unitySliceTop = s.unitySliceRight = s.unitySliceBottom = StyleKeyword.Null;
                s.unitySliceScale = StyleKeyword.Null;
            }
            scratch.Clear();
            foreach (string c in e.GetClasses())
                if (c.StartsWith("skin-", StringComparison.Ordinal))
                    scratch.Add(c);
            foreach (string c in scratch)
                e.RemoveFromClassList(c);
            scratch.Clear();
        }

        // ------------------------------------------------------------------ the cursor (:hover, :active)

        void OnOver(PointerOverEvent e)
        {
            over = e.target as VisualElement;
            Touch(over);
        }

        void OnOut(PointerOutEvent e)
        {
            var was = over;
            if (e.target == over)
                over = null;
            Touch(was);
        }

        void OnDown(PointerDownEvent e)
        {
            if (e.button != 0)
                return;
            down = e.target as VisualElement;
            Touch(down);
        }

        void OnUp(PointerUpEvent e) => LetGo();
        void OnLost(PointerCaptureOutEvent e) => LetGo();

        void LetGo()
        {
            var was = down;
            down = null;
            Touch(was);
        }

        /// <summary>An element and everything it is in, up to the root, are looked at again.</summary>
        void Touch(VisualElement from)
        {
            if (root == null || stale)
                return;
            for (var e = from; e != null; e = e.hierarchy.parent)
            {
                Skin(e);
                if (e == root)
                    break;
            }
        }

        // ------------------------------------------------------------------ what the frame's own code calls

        /// <summary>True while a piece is laid on the element in place of its own drawing: FrameIcon and FrameGlyph draw
        /// nothing then (the first line of their Paint).</summary>
        public static bool Skinned(VisualElement e) => e != null && e.ClassListContains(SkinnedClass);

        /// <summary>A place for an icon that has no element of its own yet (before a window's name): it takes no room
        /// until its piece is drawn. `cls`: the class the piece's "uss" names (".frame-win-icon").</summary>
        public static VisualElement Place(string cls)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList(PlaceClass);
            if (!string.IsNullOrEmpty(cls))
                e.AddToClassList(cls);
            e.style.display = DisplayStyle.None;
            e.style.flexShrink = 0f;
            return e;
        }

        /// <summary>Gives a line a class of its own and a place for an icon at its left end, and gives the line back: one
        /// call round the line. body.Add(FrameSkin.Mark(FrameUi.Ln(...), "stat-" + id, "frame-stat-icon")) is found by
        /// the place ".frame-ln.stat-tongue .frame-stat-icon".</summary>
        public static VisualElement Mark(VisualElement line, string cls, string placeCls)
        {
            if (line == null)
                return null;
            if (!string.IsNullOrEmpty(cls))
                line.AddToClassList(cls);
            line.Insert(0, Place(placeCls));
            return line;
        }

        static readonly Dictionary<string, string> slugs = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>Says which function a button of the rack is now, so its icon's piece finds it: the class "fn-cast_off",
        /// "fn-lie_to", "fn-wait" ... on the button. `kind`: the button's kind as the code names it ("CastOff",
        /// "LookOut"); `name`: the word on it now (the first button is Cast off in port and Lie to at sea).</summary>
        public static void Verb(VisualElement button, string kind, string name)
        {
            if (button == null || string.IsNullOrEmpty(kind))
                return;
            string key = kind == "CastOff" && name == "Lie to" ? "LieTo" : kind;
            if (!slugs.TryGetValue(key, out string cls))
                slugs[key] = cls = "fn-" + Slug(key);
            if (button.ClassListContains(cls))
                return;
            string old = null;
            foreach (string c in button.GetClasses())
                if (c.StartsWith("fn-", StringComparison.Ordinal))
                    old = c;
            if (old != null)
                button.RemoveFromClassList(old);
            button.AddToClassList(cls);
        }

        /// <summary>"CastOff" as "cast_off", "LookOut" as "look_out": a name of the code as the art list writes it.</summary>
        public static string Slug(string camel)
        {
            if (string.IsNullOrEmpty(camel))
                return "";
            var b = new StringBuilder(camel.Length + 4);
            for (int i = 0; i < camel.Length; i++)
            {
                char c = camel[i];
                if (char.IsUpper(c) && i > 0)
                    b.Append('_');
                b.Append(char.ToLowerInvariant(c));
            }
            return b.ToString();
        }
    }
}
