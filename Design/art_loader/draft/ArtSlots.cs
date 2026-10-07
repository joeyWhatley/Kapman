// PROJECT PATH: Assets/Kapman/Code/Game/Art/ArtSlots.cs
// (a draft of 7 Oct 2026, written outside Assets: Design/art_loader/PLAN.md says how to bring it in)
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// A PICTURE BY ITS SLOT (the user, 7 Oct 2026: "for the UI and really everything I will need templates for and
    /// assigned correctly and organised so I can make the sprites myself"). Every picture of the game is a slot with a
    /// fixed file name (Art/art_list.csv). This is the one place the game asks for one: a face, a good, a port's mark, a
    /// card's picture, a piece of the frame. It reads them as files, as the chart reads its tiles (MapArt): nothing is
    /// imported, the user drops a picture in and the game shows it.
    ///
    ///   WHERE      in the editor  Kapman2/Art/final and Kapman2/Art/placeholder, beside Assets
    ///              in a built game  StreamingAssets/Art/final and .../placeholder (Editor/Game/ArtBuild.cs copies them)
    ///   THE ORDER  the user's own picture, then the user's stand-in, then the placeholders (ArtFiles has the detail)
    ///   THE LOOK   pixel art: point filtering, no mipmaps, never smoothed
    ///   THE CACHE  a picture is kept while something shows it (an element on screen that <see cref="Show"/> laid it on,
    ///              or a <see cref="Keep"/>), and for <see cref="KeepSeconds"/> after it was last asked for; then let go
    ///   CHANGES    while <see cref="Watch"/> is on (the editor, a development build) the files are looked at once a
    ///              second: a picture saved again is read again into the SAME texture, so whatever shows it shows the
    ///              new one; a picture added or taken away changes what each slot falls back to, and what shows it is
    ///              laid again. <see cref="Version"/> counts up and <see cref="Changed"/> is raised.
    ///
    /// Ask when you build a thing, not every frame: <see cref="Picture(string)"/> gives the texture now, and it stays
    /// good for KeepSeconds. For an element of the screen use <see cref="Show"/>: it keeps the picture for as long as the
    /// element is on screen and follows every change. Call <see cref="Pump"/> once a frame from anything that is alive.
    /// </summary>
    public sealed class ArtSlots
    {
        // ------------------------------------------------------------------ the names of the slots (make_art_list.py)

        /// <summary>A person of a places file or of the cast: "portrait/person.dorestad.tollman.png".</summary>
        public static string Person(string id) => "portrait/person." + Safe(id) + ".png";
        /// <summary>A person of weight (a king, a bishop): "portrait/weight.offa.png".</summary>
        public static string Weight(string id) => "portrait/weight." + Safe(id) + ".png";
        /// <summary>The player: "portrait/self.frisian.man.png".</summary>
        public static string Self(string people, bool woman) => "portrait/self." + Safe(string.IsNullOrEmpty(people) ? "frisian" : people) + (woman ? ".woman.png" : ".man.png");
        /// <summary>The stand-in for anyone of a role: "portrait/role.tollman.png" (null for no role).</summary>
        public static string Role(string role) => string.IsNullOrEmpty(role) ? null : "portrait/role." + Safe(role) + ".png";
        /// <summary>A card's own picture: "card/card.crew.bored.calm.png" for the card "crew.bored.calm".</summary>
        public static string Card(string cardId) => "card/card." + Safe(cardId) + ".png";
        /// <summary>The stand-in for any card of a stage (harbour, sea, crew, long): "card/card.any.sea.png".</summary>
        public static string CardAny(string stage) => string.IsNullOrEmpty(stage) ? null : "card/card.any." + Safe(stage).ToLowerInvariant() + ".png";
        /// <summary>A site's own scene: "scene/site.dorestad.quay.png" for the site "dorestad.quay".</summary>
        public static string Site(string siteId) => "scene/site." + Safe(siteId) + ".png";
        /// <summary>The stand-in for any site of a kind: "scene/scene.any.tavern.png".</summary>
        public static string SceneAny(string kind) => string.IsNullOrEmpty(kind) ? null : "scene/scene.any." + Safe(kind) + ".png";
        public static string Good(string goodId) => "good/good." + Safe(goodId) + ".png";
        public static string PortMark(string portId) => "port_mark/portmark." + Safe(portId) + ".png";
        public static string Harbour(string portId) => "harbour/harbour." + Safe(portId) + ".png";
        /// <summary>A ship of a class from above, bow to the right: "ship_mark/shipmark.knarr.png".</summary>
        public static string ShipMark(string classId) => "ship_mark/shipmark." + Safe(classId) + ".png";
        /// <summary>A ship of a class from the side: "ship_side/ship.knarr.png".</summary>
        public static string ShipSide(string classId) => "ship_side/ship." + Safe(classId) + ".png";
        /// <summary>A piece of the frame: "hud/hud.fn_button.png" for "fn_button".</summary>
        public static string Hud(string piece) => "hud/hud." + Safe(piece) + ".png";
        /// <summary>The ship window's backdrop: place river, coast, sea or port; hour day, dusk or night.</summary>
        public static string ShipBack(string place, string hour) => "shipview/shipview.back." + Safe(place) + "." + Safe(hour) + ".png";

        /// <summary>A slot file from what a data file says: "person.x" of the kind, or the file itself ("portrait/person.x.png").</summary>
        public static string FileOf(string kind, string idOrFile)
        {
            if (string.IsNullOrEmpty(idOrFile))
                return null;
            string f = idOrFile.Replace('\\', '/');
            if (f.IndexOf('/') < 0)
                f = kind + "/" + f;
            return f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? f : f + ".png";
        }

        /// <summary>An id as make_art_list.py writes it in a file name (its safe_id): letters, digits, dot, dash and
        /// underscore stay; any run of anything else is one underscore; no dot or underscore at either end.</summary>
        public static string Safe(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "x";
            var b = new StringBuilder(id.Length);
            bool gap = false;
            foreach (char c in id)
            {
                bool keep = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '.' || c == '_' || c == '-';
                if (keep)
                {
                    if (gap)
                        b.Append('_');
                    gap = false;
                    b.Append(c);
                }
                else
                    gap = true;
            }
            if (gap)
                b.Append('_');
            string s = b.ToString().Trim('_', '.');
            return s.Length > 0 ? s : "x";
        }

        // ------------------------------------------------------------------ the game's own

        static ArtSlots shared;
        static string drawFolder;

        /// <summary>The folder that holds final, placeholder and hud_assign.json.</summary>
        public static string Folder
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "..", "Art");
#else
                return Path.Combine(Application.streamingAssetsPath, "Art");
#endif
            }
        }

        /// <summary>The game's pictures.</summary>
        public static ArtSlots Shared
        {
            get
            {
                if (shared == null)
                {
                    shared = new ArtSlots(Path.Combine(Folder, "final"), Path.Combine(Folder, "placeholder"));
                    shared.Watch = Application.isEditor || Debug.isDebugBuild;
                    shared.Files.DrawFolder = drawFolder;
                }
                return shared;
            }
        }

        /// <summary>The folder the user draws in, looked at before Art/final (null: not looked at). Off unless the
        /// editor's menu turns it on (Kapman, Art): then a picture shows as soon as it is saved, before pull_art.py.</summary>
        public static string DrawFolder
        {
            get => drawFolder;
            set
            {
                drawFolder = string.IsNullOrEmpty(value) ? null : value;
                if (shared != null && shared.Files.DrawFolder != drawFolder)
                {
                    shared.Files.DrawFolder = drawFolder;
                    shared.moved = true;                                   // (every slot is looked up again at the next look)
                }
            }
        }

        // Enter Play Mode runs without a domain reload: every play reads the pictures afresh
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetShared()
        {
            shared?.Reset();
            shared = null;
        }

        // ------------------------------------------------------------------ one set of pictures

        sealed class Entry
        {
            public string path;
            public Texture2D tex;
            public long stamp, warned;
            public int asks;
            public Dictionary<long, Sprite> sprites;
        }

        sealed class Ask
        {
            public string key;
            public string[] files;
            public bool finalOnly, drawn;
            public string path;
            public Entry entry;
            public float asked;
            public int holds;
            public List<VisualElement> shownOn;
        }

        sealed class Hold
        {
            public string[] files;
            public bool finalOnly, on, collapse, banner;
            public float scale, most = 3f;
            public Ask ask;
        }

        /// <summary>Which file a slot comes from.</summary>
        public readonly ArtFiles Files;
        /// <summary>Look at the files for changes while the game runs.</summary>
        public bool Watch = true;
        /// <summary>How often the files are looked at, in seconds.</summary>
        public float LookEvery = 1f;
        /// <summary>How long a picture that nothing shows is kept after it was last asked for.</summary>
        public float KeepSeconds = 30f;
        /// <summary>The clock (seconds): the wall clock, so a held game still sees a new picture. A test sets its own.</summary>
        public Func<float> Now = () => Time.realtimeSinceStartup;
        /// <summary>Counts up whenever a picture on disk changed what the game shows.</summary>
        public int Version { get; private set; }
        /// <summary>Raised after <see cref="Version"/> counted up.</summary>
        public event Action Changed;
        /// <summary>How many pictures are held now.</summary>
        public int Held => entries.Count;

        readonly Dictionary<string, Ask> asks = new Dictionary<string, Ask>(StringComparer.Ordinal);
        readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, long> folders = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        readonly ConditionalWeakTable<VisualElement, Hold> holds = new ConditionalWeakTable<VisualElement, Hold>();
        readonly List<string> scratch = new List<string>();
        readonly List<Ask> again = new List<Ask>();
        float nextLook;
        bool moved;

        public ArtSlots(string finalFolder, string placeholderFolder)
        {
            Files = new ArtFiles(finalFolder, placeholderFolder);
        }

        // ------------------------------------------------------------------ asking for a picture

        /// <summary>The picture of a slot ("good/good.wax.png"): the user's, else the placeholder; null with neither.</summary>
        public Texture2D Picture(string file)
        {
            if (string.IsNullOrEmpty(file))
                return null;
            if (asks.TryGetValue(file, out var a))                        // (asked before: no new strings)
            {
                a.asked = Now();
                return Load(a);
            }
            return Load(Get(new[] { file }, false));
        }

        /// <summary>The picture of a slot, or of its stand-in where the slot has none: a person and their role, a card and
        /// its stage, a site and its kind (ruling 13; ArtFiles has the order).</summary>
        public Texture2D Picture(string file, string standIn) => Load(Get(Pack(file, standIn), false));

        /// <summary>A person's portrait, else the stand-in of their role.</summary>
        public Texture2D Portrait(string personFile, string role) => Picture(personFile, Role(role));
        /// <summary>A card's own picture, else the stand-in of its stage, else null (a card needs no picture).</summary>
        public Texture2D CardPicture(string cardId, string stage) => Picture(Card(cardId), CardAny(stage));
        /// <summary>A site's own scene, else the stand-in of its kind.</summary>
        public Texture2D Scene(string siteId, string kind) => Picture(Site(siteId), SceneAny(kind));

        /// <summary>Only the user's own picture of a slot (Art/final), or null: the frame's pieces, which are used only
        /// once they are drawn.</summary>
        public Texture2D Final(string file) => string.IsNullOrEmpty(file) ? null : Load(Get(new[] { file }, true));

        /// <summary>Has the user drawn this slot? (No picture is read to answer.)</summary>
        public bool Drawn(string file) => !string.IsNullOrEmpty(file) && Get(new[] { file }, true).path != null;

        /// <summary>The file a slot and its stand-ins are shown from now (null: none): for tests and for looking into it.</summary>
        public string Source(params string[] files) => Get(Pack(files), false).path;

        /// <summary>A slot's picture as a sprite, cut in nine if `slice` is given (left, top, right, bottom in pixels, as
        /// Art/hud_assign.json has them). For a SpriteRenderer or a uGUI Image; UI Toolkit takes the texture.</summary>
        public Sprite SpriteOf(string file, int[] slice = null, float pixelsPerUnit = 1f, bool finalOnly = false)
        {
            if (string.IsNullOrEmpty(file))
                return null;
            var a = Get(new[] { file }, finalOnly);
            var tex = Load(a);
            if (tex == null)
                return null;
            var border = Border(slice, tex.width, tex.height);
            long key = unchecked(((long)border.x) | ((long)border.y << 12) | ((long)border.z << 24) | ((long)border.w << 36) ^ ((long)pixelsPerUnit.GetHashCode() << 48));
            var e = a.entry;
            if (e.sprites == null)
                e.sprites = new Dictionary<long, Sprite>();
            if (e.sprites.TryGetValue(key, out var sprite) && sprite != null)
                return sprite;
            sprite = UnityEngine.Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), Mathf.Max(0.01f, pixelsPerUnit), 0,
                                               SpriteMeshType.FullRect, border);
            sprite.name = tex.name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            e.sprites[key] = sprite;
            return sprite;
        }

        /// <summary>The cuts of Art/hud_assign.json (left, top, right, bottom, in the picture's pixels from its top left) as
        /// a sprite's border (left, bottom, right, top). No cuts, or cuts that do not fit the picture: none.</summary>
        public static Vector4 Border(int[] slice, int width, int height)
        {
            if (slice == null || slice.Length != 4)
                return Vector4.zero;
            int l = slice[0], t = slice[1], r = slice[2], b = slice[3];
            if (l < 0 || t < 0 || r < 0 || b < 0 || l + r >= width || t + b >= height)
                return Vector4.zero;
            return new Vector4(l, b, r, t);
        }

        /// <summary>Keeps a slot's picture until <see cref="Release"/> (something that is always on screen and lays the
        /// texture on by itself: the frame's pieces). Gives the picture, or null.</summary>
        public Texture2D Keep(string file, bool finalOnly = false)
        {
            if (string.IsNullOrEmpty(file))
                return null;
            var a = Get(new[] { file }, finalOnly);
            a.holds++;
            return Load(a);
        }

        public void Release(string file, bool finalOnly = false)
        {
            if (!string.IsNullOrEmpty(file) && asks.TryGetValue(KeyOf(new[] { file }, finalOnly), out var a) && a.holds > 0)
                a.holds--;
        }

        // ------------------------------------------------------------------ a picture on an element of the screen

        /// <summary>
        /// Lays a slot's picture on an element as its background (the stand-in where the slot has none). The picture is
        /// kept for as long as the element is on screen, and the element shows the new one when the file changes or
        /// another file takes its place. True when there is a picture now. Showing another slot on the same element
        /// replaces the first.
        /// </summary>
        public bool Show(VisualElement el, string file, string standIn = null) => Show(el, Pack(file, standIn), false, 0f, false, false);

        /// <summary>As <see cref="Show"/>, but only the user's own picture counts (the frame's pieces).</summary>
        public bool ShowFinal(VisualElement el, string file) => Show(el, Pack(file, null), true, 0f, false, false);

        /// <summary>
        /// The element BECOMES the picture: it takes the picture's size times `scale` (whole numbers keep the pixels
        /// even), loses its border, its rounded corners and its ground colour. With no picture it is as its stylesheet
        /// has it. For a mark on the chart (a port, a ship) and an icon in a line.
        /// </summary>
        public bool ShowSized(VisualElement el, float scale, string file, string standIn = null) => Show(el, Pack(file, standIn), false, Mathf.Max(0.01f, scale), false, false);

        /// <summary>As <see cref="ShowSized"/>, but only the user's own picture counts: a piece of the frame that sits on
        /// the chart (a landmark's mark), which keeps its stylesheet look until it is drawn.</summary>
        public bool ShowFinalSized(VisualElement el, float scale, string file) => Show(el, Pack(file, null), true, Mathf.Max(0.01f, scale), false, false);

        /// <summary>A new element that is a slot's picture at its own size times `scale`; with no picture it takes no room.</summary>
        public VisualElement Element(string file, string standIn = null, float scale = 1f)
        {
            var el = new VisualElement { pickingMode = PickingMode.Ignore };
            el.AddToClassList("art-slot");
            el.style.flexShrink = 0f;
            Show(el, Pack(file, standIn), false, Mathf.Max(0.01f, scale), true, false);
            return el;
        }

        /// <summary>A new element for a wide picture (a harbour view, a scene, a card's picture) across the full width of
        /// what it is put in: at a whole multiple of its own pixels where it fits (no more than `maxScale`), shrunk to
        /// the width where it does not, as tall as its shape asks. With no picture it takes no room.</summary>
        public VisualElement Banner(string file, string standIn = null, float maxScale = 3f)
        {
            var el = new VisualElement { pickingMode = PickingMode.Ignore };
            el.AddToClassList("art-banner");
            el.style.width = Length.Percent(100f);
            el.style.flexShrink = 0f;
            Show(el, Pack(file, standIn), false, 0f, true, true);
            if (holds.TryGetValue(el, out var h))
                h.most = Mathf.Max(1f, maxScale);
            el.RegisterCallback<GeometryChangedEvent>(_ => Fit(el));
            return el;
        }

        /// <summary>Puts a slot's picture at the left end of a line (a good's icon before its name) and gives the line
        /// back, so it is one call round the line: body.Add(art.Before(FrameUi.Ln(...), ArtSlots.Good(id))).</summary>
        public VisualElement Before(VisualElement line, string file, string standIn = null, float scale = 1f)
        {
            if (line == null)
                return null;
            var icon = Element(file, standIn, scale);
            icon.style.marginRight = 8f;
            line.Insert(0, icon);
            return line;
        }

        /// <summary>Is a picture laid on this element now? (A face place draws its empty shape when not.)</summary>
        public bool Shows(VisualElement el) =>
            el != null && holds.TryGetValue(el, out var h) && h.ask != null && h.ask.entry != null && h.ask.entry.tex != null;

        /// <summary>Takes a picture off an element again.</summary>
        public void Clear(VisualElement el)
        {
            if (el == null || !holds.TryGetValue(el, out var h))
                return;
            Unbind(el, h);
            h.files = null;
            Lay(el, h, null);
        }

        bool Show(VisualElement el, string[] files, bool finalOnly, float scale, bool collapse, bool banner)
        {
            if (el == null)
                return false;
            if (!holds.TryGetValue(el, out var h))
            {
                h = new Hold();
                holds.Add(el, h);
                el.RegisterCallback<AttachToPanelEvent>(OnAttach);
                el.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            }
            Unbind(el, h);
            h.files = files;
            h.finalOnly = finalOnly;
            h.scale = scale;
            h.collapse = collapse;
            h.banner = banner;
            return Bind(el, h);
        }

        bool Bind(VisualElement el, Hold h)
        {
            var a = Get(h.files, h.finalOnly);
            h.ask = a;
            if (a.shownOn == null)
                a.shownOn = new List<VisualElement>();
            a.shownOn.Add(el);
            h.on = el.panel != null;
            if (h.on)
                a.holds++;
            var tex = Load(a);
            Lay(el, h, tex);
            return tex != null;
        }

        void Unbind(VisualElement el, Hold h)
        {
            if (h.ask == null)
                return;
            h.ask.shownOn?.Remove(el);
            if (h.on && h.ask.holds > 0)
                h.ask.holds--;
            h.on = false;
            h.ask = null;
        }

        void OnAttach(AttachToPanelEvent e)
        {
            // on screen (again): the picture is held from now, and laid afresh (it may have been let go meanwhile)
            if (e.target is VisualElement el && holds.TryGetValue(el, out var h) && h.files != null)
            {
                Unbind(el, h);
                Bind(el, h);
            }
        }

        void OnDetach(DetachFromPanelEvent e)
        {
            if (e.target is VisualElement el && holds.TryGetValue(el, out var h))
                Unbind(el, h);
        }

        static void Lay(VisualElement el, Hold h, Texture2D tex)
        {
            var s = el.style;
            s.backgroundImage = tex != null ? new StyleBackground(tex) : new StyleBackground(StyleKeyword.Null);
            el.MarkDirtyRepaint();                                        // (an element that draws a stand-in of its own looks again)
            if (h.collapse)
                s.display = tex != null ? DisplayStyle.Flex : DisplayStyle.None;
            if (h.scale > 0f)
            {
                if (tex != null)
                {
                    s.width = tex.width * h.scale;
                    s.height = tex.height * h.scale;
                    s.backgroundSize = new StyleBackgroundSize(new BackgroundSize(Length.Percent(100f), Length.Percent(100f)));
                    s.backgroundColor = Color.clear;
                    s.borderLeftWidth = s.borderTopWidth = s.borderRightWidth = s.borderBottomWidth = 0f;
                    s.borderTopLeftRadius = s.borderTopRightRadius = s.borderBottomLeftRadius = s.borderBottomRightRadius = 0f;
                }
                else
                {
                    s.width = s.height = StyleKeyword.Null;
                    s.backgroundSize = StyleKeyword.Null;
                    s.backgroundColor = StyleKeyword.Null;
                    s.borderLeftWidth = s.borderTopWidth = s.borderRightWidth = s.borderBottomWidth = StyleKeyword.Null;
                    s.borderTopLeftRadius = s.borderTopRightRadius = s.borderBottomLeftRadius = s.borderBottomRightRadius = StyleKeyword.Null;
                }
            }
            if (h.banner)
            {
                s.backgroundRepeat = new StyleBackgroundRepeat(new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat));
                s.backgroundPositionX = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Center));
                s.backgroundPositionY = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Center));
            }
        }

        /// <summary>A banner follows the width it was given: whole multiples of its pixels where they fit.</summary>
        void Fit(VisualElement el)
        {
            if (!holds.TryGetValue(el, out var h) || h.ask == null || h.ask.entry == null || h.ask.entry.tex == null)
                return;
            var tex = h.ask.entry.tex;
            float room = el.resolvedStyle.width;
            if (float.IsNaN(room) || room < 1f || tex.width < 1)
                return;
            float k = Mathf.Min(Mathf.Floor(room / tex.width), h.most);
            float w = k >= 1f ? tex.width * k : room, t = k >= 1f ? tex.height * k : room * tex.height / tex.width;
            el.style.height = t;
            el.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(new Length(w), new Length(t)));
        }

        // ------------------------------------------------------------------ each frame: changes, and letting go

        /// <summary>Once a frame, from anything that is alive (the frame's Update): looks at the files and lets go of
        /// what is no longer shown, at most every <see cref="LookEvery"/> seconds.</summary>
        public void Pump()
        {
            float now = Now();
            if (now < nextLook)
                return;
            nextLook = now + Mathf.Max(0.05f, LookEvery);
            Look();
        }

        /// <summary>Looks now: a picture written again is read again into its own texture; a picture added or taken away
        /// changes what each slot is shown from; what nothing shows any more is let go.</summary>
        public void Look()
        {
            bool changed = false;
            if (Watch)
            {
                // a picture added to a folder, taken from it or renamed in it: the folder's own time of writing moves
                scratch.Clear();
                scratch.AddRange(folders.Keys);
                foreach (string dir in scratch)
                {
                    long s = ArtFiles.FolderStamp(dir);
                    if (s != folders[dir])
                    {
                        folders[dir] = s;
                        moved = true;
                    }
                }
                // a picture written again
                again.Clear();
                foreach (var e in entries.Values)
                {
                    long s = ArtFiles.Stamp(e.path);
                    if (s == e.stamp)
                        continue;
                    if (s == 0L)
                    {
                        moved = true;                                      // (gone: whoever showed it looks again)
                        continue;
                    }
                    int w = e.tex != null ? e.tex.width : 0, ht = e.tex != null ? e.tex.height : 0;
                    if (!Read(e))
                        continue;
                    changed = true;
                    if (e.tex.width != w || e.tex.height != ht)
                        DropSprites(e);
                    // (whatever shows it is told to draw again, and to take its new size if that changed)
                    foreach (var a in asks.Values)
                        if (a.entry == e)
                            again.Add(a);
                }
                if (moved)
                {
                    moved = false;
                    foreach (var a in asks.Values)
                        if (Resolve(a))
                        {
                            changed = true;
                            if (!again.Contains(a))
                                again.Add(a);
                        }
                }
                foreach (var a in again)
                    Relay(a);
                again.Clear();
            }

            // what nothing shows and nobody asked for lately is let go
            float now = Now();
            scratch.Clear();
            foreach (var a in asks.Values)
                if (a.holds <= 0 && now - a.asked > KeepSeconds)
                    scratch.Add(a.key);
            foreach (string key in scratch)
            {
                var a = asks[key];
                asks.Remove(key);
                if (a.entry != null)
                    a.entry.asks--;
                a.entry = null;
                if (a.shownOn != null)
                {
                    // (elements that never came on screen: they ask again if they ever do)
                    foreach (var el in a.shownOn)
                        if (holds.TryGetValue(el, out var h) && h.ask == a)
                            h.ask = null;
                    a.shownOn.Clear();
                }
            }
            scratch.Clear();
            foreach (var e in entries.Values)
                if (e.asks <= 0)
                    scratch.Add(e.path);
            foreach (string path in scratch)
            {
                var e = entries[path];
                entries.Remove(path);
                DropSprites(e);
                Kill(e.tex);
            }

            if (changed)
            {
                Version++;
                Changed?.Invoke();
            }
        }

        /// <summary>Lets go of everything (each play, and a test's end).</summary>
        public void Reset()
        {
            foreach (var e in entries.Values)
            {
                DropSprites(e);
                Kill(e.tex);
            }
            entries.Clear();
            foreach (var a in asks.Values)
            {
                a.entry = null;
                a.shownOn?.Clear();
            }
            asks.Clear();
            folders.Clear();
            nextLook = 0f;
            moved = false;
        }

        // ------------------------------------------------------------------ inside

        static string[] Pack(string file, string standIn) =>
            string.IsNullOrEmpty(standIn) ? new[] { file ?? "" } : new[] { file ?? "", standIn };

        static string[] Pack(string[] files)
        {
            if (files == null)
                return new string[0];
            var list = new List<string>(files.Length);
            foreach (string f in files)
                if (!string.IsNullOrEmpty(f))
                    list.Add(f);
            return list.ToArray();
        }

        static string KeyOf(string[] files, bool finalOnly)
        {
            if (!finalOnly && files.Length == 1)
                return files[0];
            return (finalOnly ? "!" : "") + string.Join("|", files);
        }

        Ask Get(string[] files, bool finalOnly)
        {
            string key = KeyOf(files, finalOnly);
            if (!asks.TryGetValue(key, out var a))
            {
                a = new Ask { key = key, files = files, finalOnly = finalOnly };
                a.path = Files.Find(files, finalOnly, out a.drawn);
                asks[key] = a;
                scratch.Clear();
                foreach (string f in files)
                    Files.Folders(f, scratch);
                foreach (string dir in scratch)
                    if (!folders.ContainsKey(dir))
                        folders[dir] = ArtFiles.FolderStamp(dir);
                scratch.Clear();
            }
            a.asked = Now();
            return a;
        }

        /// <summary>Looks up again which file an ask is shown from. True when it changed.</summary>
        bool Resolve(Ask a)
        {
            string was = a.path;
            a.path = Files.Find(a.files, a.finalOnly, out a.drawn);
            if (string.Equals(a.path, was, StringComparison.OrdinalIgnoreCase))
                return false;
            if (a.entry != null)
                a.entry.asks--;
            a.entry = null;
            return true;
        }

        Texture2D Load(Ask a)
        {
            if (a.path == null)
                return null;
            if (a.entry == null)
            {
                if (!entries.TryGetValue(a.path, out var e))
                {
                    e = new Entry { path = a.path };
                    Read(e);
                    entries[a.path] = e;
                }
                e.asks++;
                a.entry = e;
            }
            return a.entry.tex;
        }

        /// <summary>Whatever shows an ask is given its picture again.</summary>
        void Relay(Ask a)
        {
            if (a.shownOn == null || a.shownOn.Count == 0)
                return;
            var tex = Load(a);
            foreach (var el in a.shownOn)
                if (holds.TryGetValue(el, out var h))
                {
                    Lay(el, h, tex);
                    if (h.banner)
                        Fit(el);
                }
        }

        /// <summary>Reads a file into its entry's texture (the same texture when it is read again, so whatever shows it
        /// shows the new picture). A file that is half saved or is no picture leaves what was there, and is tried again
        /// at the next look. True when the picture was read.</summary>
        bool Read(Entry e)
        {
            long stamp = ArtFiles.Stamp(e.path);
            byte[] bytes;
            try
            {
                using (var f = new FileStream(e.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    bytes = new byte[f.Length];
                    int got = 0;
                    while (got < bytes.Length)
                    {
                        int n = f.Read(bytes, got, bytes.Length - got);
                        if (n <= 0)
                            break;
                        got += n;
                    }
                    if (got != bytes.Length)
                        return false;
                }
            }
            catch (Exception x) when (x is IOException || x is UnauthorizedAccessException)
            {
                return false;                                              // (a drawing program has it: next look)
            }
            if (!WholePng(bytes))
            {
                if (e.warned != stamp)
                {
                    e.warned = stamp;
                    Debug.LogWarning("Kapman art: " + e.path + " is not a whole PNG (half saved, or another format under a .png name). It is left as it was.");
                }
                return false;
            }
            var tex = e.tex;
            bool made = tex == null;
            if (made)
                tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave, name = Path.GetFileName(e.path) };
            // (kept readable while the files are watched, so the same texture can be read into again)
            if (!tex.LoadImage(bytes, !Watch))
            {
                if (made)
                    Kill(tex);
                return false;
            }
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            e.tex = tex;
            e.stamp = stamp;
            return true;
        }

        /// <summary>A PNG that is all there: its eight bytes at the start and its IEND chunk at the end.</summary>
        public static bool WholePng(byte[] b)
        {
            if (b == null || b.Length < 8 + 25 + 12)
                return false;
            if (b[0] != 0x89 || b[1] != 0x50 || b[2] != 0x4E || b[3] != 0x47 || b[4] != 0x0D || b[5] != 0x0A || b[6] != 0x1A || b[7] != 0x0A)
                return false;
            int n = b.Length;
            return b[n - 8] == 0x49 && b[n - 7] == 0x45 && b[n - 6] == 0x4E && b[n - 5] == 0x44;
        }

        static void DropSprites(Entry e)
        {
            if (e.sprites == null)
                return;
            foreach (var s in e.sprites.Values)
                Kill(s);
            e.sprites = null;
        }

        static void Kill(UnityEngine.Object o)
        {
            if (o == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(o);
            else
                UnityEngine.Object.DestroyImmediate(o);
        }
    }
}
