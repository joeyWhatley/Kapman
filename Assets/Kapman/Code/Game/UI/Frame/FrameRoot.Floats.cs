using System;
using System.Collections.Generic;
using Kapman.Encounters;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>One choice of a floating window: its words, a tag before them ("Nerve"), its odds as a word ("likely"),
    /// whether it can be chosen (else the rule that forbids it, in words), whether Enter takes it, and what it does.</summary>
    public sealed class FloatOption
    {
        public string label = "", tag = "", odds = "", rule = "";
        public bool enabled = true, isDefault;
        public Action pick;
    }

    /// <summary>What a floating window shows. Open one with FrameRoot.OpenFloat; open the same id again to change it in place.</summary>
    /// <summary>Who says a line of a window: the line under the title ("Ravn, crewman \u00b7 easy"), who they are, whose
    /// face (an id or a name) and its picture. All "" for the teller: nobody's face, nobody's name.</summary>
    public sealed class FloatVoice
    {
        public string who = "", about = "", face = "", faceSlot = "";
        public bool Teller => who.Length == 0 && face.Length == 0;
    }

    public sealed class FloatSpec
    {
        /// <summary>One window an id: opening it again changes the window that is up.</summary>
        public string id = "";
        public string title = "";
        /// <summary>The speaker's line under the title ("Thiadger, the toll man · easy"), and their name and what they are
        /// for the portrait's place (whoever is speaking stands there while they speak). "" for none.</summary>
        public string who = "", speaker = "", speakerLine = "";
        /// <summary>Who they are, in a line under that: their people, their faith, their nature ("" for none).</summary>
        public string about = "";
        /// <summary>Whose face place it has (a name or an id; null: no face).</summary>
        public string face;
        /// <summary>The picture for that face where it is known ("portrait/role.tollman.png"); "": found by the name.</summary>
        public string faceSlot = "";

        /// <summary>
        /// WHO SAYS EACH LINE (8 Oct 2026: a card with several speakers showed every line under the face and name of
        /// the first). For a line (by its words): the one who says it. As the lines are said, the face, the name and
        /// what they are change with the speaker; a line of the teller's has no face. A line with no entry here is
        /// the window's own speaker's (who, face, about).
        /// </summary>
        public readonly Dictionary<string, FloatVoice> voices = new Dictionary<string, FloatVoice>();
        /// <summary>Lines that are not said by the one who speaks (the player's own choice, told back): shown as plain
        /// words, not as speech.</summary>
        public readonly HashSet<string> plain = new HashSet<string>();
        /// <summary>The player's own choice, told back above what came of it: small, and not one of the lines that
        /// are said one at a time ("" for none).</summary>
        public string chose = "";
        /// <summary>Lines that tell what came of a choice (its written end): the teller's, in the full ink.</summary>
        public readonly HashSet<string> ending = new HashSet<string>();
        /// <summary>The words, in order; an unwritten one (a placeholder for the writers) is shown small and pale.</summary>
        public readonly List<(string text, bool written)> lines = new List<(string, bool)>();

        /// <summary>
        /// WHAT A LINE POINTS AT AS IT IS SAID (the user, 8 Oct 2026: "when Ravn is explaining what to do and how to
        /// play can you highlight what he's talking about"). For a line (by its words): from which letter on, which
        /// parts of the frame have the frame's pointer round them. Filled by <see cref="Say"/>.
        /// </summary>
        public readonly Dictionary<string, List<(int at, string parts)>> points = new Dictionary<string, List<(int, string)>>();

        /// <summary>
        /// Adds a written line that points at parts of the frame as it is said. In the text, [[>names]] means "from
        /// here on, this is about those parts": the names are the parts' names in the frame's layout file (Frame.uxml:
        /// verb-voyage, figure-silver, ship, log, speed-pause...), several with spaces between them (one frame is drawn
        /// round them all), or text:Words for the button whose words begin so (text:Mend it). [[>]] points at nothing.
        /// The marks are taken out of what is shown.
        ///   spec.Say("She wants three more men. [[>verb-alehouse]]They sit in the alehouse.");
        /// </summary>
        public void Say(string text)
        {
            var clean = new System.Text.StringBuilder();
            var cues = new List<(int at, string parts)>();
            int i = 0;
            text = text ?? "";
            while (i < text.Length)
            {
                int open = text.IndexOf("[[>", i, StringComparison.Ordinal);
                int close = open >= 0 ? text.IndexOf("]]", open + 3, StringComparison.Ordinal) : -1;
                if (close < 0)
                {
                    clean.Append(text, i, text.Length - i);
                    break;
                }
                clean.Append(text, i, open - i);
                cues.Add((clean.Length, text.Substring(open + 3, close - open - 3).Trim()));
                i = close + 2;
            }
            string line = clean.ToString();
            lines.Add((line, true));
            if (cues.Count > 0)
                points[line] = cues;
        }
        /// <summary>Small print under the words; and what came of the choice, once made.</summary>
        public string small = "", result = "";
        /// <summary>Its small print is the scene's own description (a card's, the looks of somebody spoken to): where
        /// the talk is paced, it is said first, as the teller's own boxes, and nothing stands under the words.</summary>
        public bool sceneFirst;
        public readonly List<FloatOption> options = new List<FloatOption>();
        /// <summary>It cannot be shut without an answer (a card; a talk with no way back).</summary>
        public bool mustAnswer;
        /// <summary>The game's clock waits while it is up.</summary>
        public bool holdsClock;
        /// <summary>A card left unanswered with the clock running: how long it will wait yet and what it then does by
        /// itself (null: it does nothing by itself). Read each frame: the hours run down.</summary>
        public Func<string> waits;
        /// <summary>What shutting it does (the cross, Esc), when it may be shut. Null: it just goes.</summary>
        public Action shut;
        /// <summary>Its own content in place of the lines (a line that turns from a tongue into English, a picture).</summary>
        public VisualElement content;
        /// <summary>A class of the stylesheet for this window alone (a wider one, a taller one). "" for none.</summary>
        public string look = "";
        /// <summary>Lines that come up in the speaker's own tongue and turn into English piece by piece (FloatTurn), over
        /// the plain lines. Only ever real lines of the tongues file.</summary>
        public readonly List<FloatTurn> turns = new List<FloatTurn>();
        /// <summary>The speaker's tongue (an id of the tongues file; "" for none): its words in the lines are set apart,
        /// with their meaning on hover. And words set apart on purpose, whatever their tongue (a greeting put in).</summary>
        public string tongue = "";
        public readonly List<TongueWord> marks = new List<TongueWord>();
    }

    /// <summary>
    /// FLOATING WINDOWS (the user, 7 Oct 2026: "There will be lots of popups"; "usually the conversation will be an event
    /// pop up in the middle of the screen"). Cards and talk open over the middle of the chart: the speaker's face place,
    /// the words, the choices (each with its odds as a word). Several may be up at once, each a little down and to the
    /// right of the last; a click brings one to the front; the number keys choose on the front one, Enter takes its
    /// marked choice, Esc shuts it if it may be shut. A card must be answered, and the clock waits while it does.
    ///
    /// What opens them by itself, on the chart:
    ///   - THE CARD IN PLAY (EncounterRunner.Director.Current), in the old EncounterPanel's place;
    ///   - A TALK: whatever the graphs put in the text panel that is not the port's own menu (the waterfront's talk, an
    ///     errand's outcome, a meeting at sea), in the port's numbered menu's place (the menu itself is the Port window's);
    ///   - AN URGENT ALERT (raiders made out, a guard boat, aground): it holds the game until it is answered.
    /// Anything else opens one with <see cref="OpenFloat"/>.
    /// </summary>
    public partial class FrameRoot
    {
        sealed class Float
        {
            public FloatSpec spec;
            public VisualElement el;
            public Label hint;
            public int slot;
            public float hintUntil;
            /// <summary>The player went on with it still open: it holds the clock no more, and waits.</summary>
            public bool released;
            /// <summary>The talking text of its lines (null: they stand whole), the lines already said, and the one being
            /// said with how far it has got: a window drawn again goes on where it was.</summary>
            public TextReveal reveal;
            public readonly List<string> said = new List<string>();
            public string saying = "";
            public int sayingShown;
            /// <summary>Its choices (put away while there is more to be said), and whether there is.</summary>
            public VisualElement optionsBox;
            public bool more;
            /// <summary>The mark that the box stands whole and waits for the key (the part named "more" of Parts/Float.uxml;
            /// null: nothing is said a box at a time).</summary>
            public FrameGlyph moreMark;
            /// <summary>The face, the name and the nature shown for the one who speaks, and the line they were last set for.</summary>
            public FrameFace faceEl;
            public Label whoEl, aboutEl;
            public string voiceOf;
        }



        // ------------------------------------------------------------------ who says the line being said

        /// <summary>The speaker of the line a window is saying now (the last one when all are said), or null where its
        /// lines have no speakers of their own or the window is not said aloud.</summary>
        static FloatVoice VoiceNow(Float f)
        {
            var r = f.reveal;
            if (r == null || r.Count == 0 || f.spec.voices.Count == 0)
                return null;
            int line = r.Done ? r.Count - 1 : r.Index;
            return f.spec.voices.TryGetValue(r.Key(line), out var v) ? v : null;
        }

        /// <summary>The face, the name and the nature of a window follow the one who says the line being said.</summary>
        static void Voice(Float f)
        {
            if (f.faceEl == null || f.reveal == null || f.reveal.Count == 0)
                return;
            int line = f.reveal.Done ? f.reveal.Count - 1 : f.reveal.Index;
            string key = f.reveal.Key(line);
            if (key == f.voiceOf)
                return;
            f.voiceOf = key;
            var spec = f.spec;
            var v = VoiceNow(f) ?? new FloatVoice { who = spec.who ?? "", about = spec.about ?? "", face = spec.face ?? "", faceSlot = spec.faceSlot ?? "" };
            // (the teller has no face and no name: the places stay, so the words do not jump)
            f.faceEl.style.visibility = v.Teller ? Visibility.Hidden : Visibility.Visible;
            if (!v.Teller)
            {
                f.faceEl.Who = v.face;
                if (v.faceSlot.Length > 0)
                    f.faceEl.Slot = v.faceSlot;
            }
            f.whoEl.text = v.who;
            f.whoEl.style.visibility = v.who.Length > 0 ? Visibility.Visible : Visibility.Hidden;
            f.aboutEl.text = v.about;
            f.aboutEl.style.display = v.about.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>Who the front window shows as speaking now ("" where nobody is shown: for a test, a tool).</summary>
        public string SpeakerShown
        {
            get
            {
                if (floats.Count == 0)
                    return "";
                var f = floats[floats.Count - 1];
                return f.whoEl != null && f.whoEl.resolvedStyle.visibility == Visibility.Visible ? f.whoEl.text : "";
            }
        }

        // ------------------------------------------------------------------ what the one who speaks is talking about
        VisualElement pointer;

        /// <summary>The parts of the frame the pointer is round now, as the line names them ("" for none).</summary>
        public string PointedAt { get; private set; } = "";
        /// <summary>The pointer's own element (for a test, a tool).</summary>
        public VisualElement Pointer => pointer;

        /// <summary>What the line being said in a window points at now: the last mark its words have come to (all of them
        /// when the line stands whole and waits); null when the line has none, or the window is not said aloud.</summary>
        static string PointedBy(Float f)
        {
            var r = f.reveal;
            if (r == null || r.Count == 0 || f.spec.points.Count == 0)
                return null;
            int line = r.Done ? r.Count - 1 : r.Index;
            if (!f.spec.points.TryGetValue(r.Key(line), out var cues))
                return null;
            int shown = r.Done || r.Waiting ? int.MaxValue : r.Shown;
            string parts = null;
            foreach (var (at, names) in cues)
                if (at <= shown)
                    parts = names;
            return parts;
        }

        /// <summary>A part of the frame by a mark's name: its name in the layout file, or text:Words for the button whose
        /// words begin so. Null if there is none such, or it is not on the screen.</summary>
        VisualElement PointedPart(string partName)
        {
            VisualElement e = null;
            if (partName.StartsWith("text:", StringComparison.Ordinal))
            {
                string words = partName.Substring(5);
                root.Query<Button>().ForEach(b =>
                {
                    if (e == null && b.text != null && b.text.StartsWith(words, StringComparison.Ordinal) && b.resolvedStyle.display != DisplayStyle.None && b.worldBound.width > 1f)
                        e = b;
                });
            }
            else
                e = root.Q<VisualElement>(partName);
            if (e == null || e.panel == null)
                return null;
            // (not a thing that is put away, nor one inside a thing put away: the tab of the window that is not shown)
            for (var up = e; up != null && up != root; up = up.parent)
                if (up.resolvedStyle.display == DisplayStyle.None)
                    return null;
            var box = e.worldBound;
            return float.IsNaN(box.width) || box.width < 1f || box.height < 1f ? null : e;
        }

        /// <summary>Each frame: the pointer goes round what the front-most pointing line is about, or is put away.</summary>
        void PointerUpdate()
        {
            if (pointer == null)
                return;
            string parts = null;
            for (int i = floats.Count - 1; i >= 0 && parts == null; i--)
                parts = PointedBy(floats[i]);
            bool any = false;
            float x0 = 0f, y0 = 0f, x1 = 0f, y1 = 0f;
            if (!string.IsNullOrEmpty(parts))
                foreach (string partName in parts.Split(' '))
                {
                    var e = partName.Length > 0 ? PointedPart(partName) : null;
                    if (e == null)
                        continue;
                    var b = e.worldBound;
                    x0 = any ? Mathf.Min(x0, b.xMin) : b.xMin;
                    y0 = any ? Mathf.Min(y0, b.yMin) : b.yMin;
                    x1 = any ? Mathf.Max(x1, b.xMax) : b.xMax;
                    y1 = any ? Mathf.Max(y1, b.yMax) : b.yMax;
                    any = true;
                }
            if (!any)
            {
                if (PointedAt.Length > 0)
                {
                    pointer.style.display = DisplayStyle.None;
                    PointedAt = "";
                }
                return;
            }
            // a little outside the thing itself, in the frame's own pixels
            const float pad = 6f;
            var corner = root.WorldToLocal(new Vector2(x0, y0));
            var far = root.WorldToLocal(new Vector2(x1, y1));
            pointer.style.left = Mathf.Round(corner.x - pad);
            pointer.style.top = Mathf.Round(corner.y - pad);
            pointer.style.width = Mathf.Round(far.x - corner.x + pad * 2f);
            pointer.style.height = Mathf.Round(far.y - corner.y + pad * 2f);
            pointer.style.display = DisplayStyle.Flex;
            pointer.BringToFront();
            // it blinks in steps, not smoothly (as the marks of the first morning do)
            float k = Mathf.Floor(Mathf.PingPong(Time.unscaledTime * 2.6f, 1f) * 2.999f) / 2f;
            pointer.style.opacity = 0.55f + 0.45f * k;
            PointedAt = parts;
        }

        /// <summary>The front window's talking text, if it is still being said (for a test, a tool).</summary>
        public TextReveal FrontReveal => floats.Count > 0 ? floats[floats.Count - 1].reveal : null;

        /// <summary>Where the mark that the front window's box of talk waits for the key stands on the screen now (the
        /// part "more" of Parts/Float.uxml; it blinks). An empty place: it is not on the screen now.</summary>
        public Rect MoreMarkPlace
        {
            get
            {
                var mark = floats.Count > 0 ? floats[floats.Count - 1].moreMark : null;
                if (mark == null || mark.panel == null || mark.resolvedStyle.display == DisplayStyle.None || mark.resolvedStyle.visibility != Visibility.Visible)
                    return default;
                return mark.worldBound;
            }
        }

        VisualElement floatLayer;
        readonly List<Float> floats = new List<Float>();                 // in order of depth: the last is the front
        int floatSlots;
        EncounterDirector hooked;
        EncounterSession cardShown;
        bool cardChosen;
        int talkVersion = -1;
        string talkShape = "", talkKey = "";
        Label talkLine;                                               // the line under a talk's title (it may run: a chase's numbers)
        EncounterPanel oldCardPanel;
        bool oldCardPanelOff;
        readonly Dictionary<string, double> alertsAnswered = new Dictionary<string, double>();
        readonly HashSet<string> alertsTold = new HashSet<string>();

        /// <summary>A floating window that holds the clock is up (PanelClock asks).</summary>
        public static bool FloatHoldsClock
        {
            get
            {
                var f = Current;
                if (f == null)
                    return false;
                foreach (var w in f.floats)
                    if (w.spec.holdsClock && !w.released)
                        return true;
                return false;
            }
        }

        /// <summary>How many floating windows are up.</summary>
        public int FloatCount => floats.Count;
        /// <summary>The ids of the floating windows that are up, back to front.</summary>
        public IEnumerable<string> FloatIds
        {
            get
            {
                foreach (var f in floats)
                    yield return f.spec.id;
            }
        }

        public bool FloatOpen(string id) => floats.Exists(f => f.spec.id == id);

        // ------------------------------------------------------------------ opening and closing

        /// <summary>Opens a floating window, or changes the one with the same id in place. A new one comes to the front.</summary>
        public void OpenFloat(FloatSpec spec)
        {
            if (spec == null || floatLayer == null)
                return;
            var f = floats.Find(x => x.spec.id == spec.id);
            bool fresh = f == null;
            if (f == null)
            {
                f = new Float { slot = floatSlots++ % 5 };
                floats.Add(f);
            }
            f.spec = spec;
            f.el?.RemoveFromHierarchy();
            f.el = Build(f);
            if (fresh && Showtime.On)
            {
                // (a new window eases in; one drawn again is simply there)
                var el = f.el;
                el.AddToClassList("easing");
                el.schedule.Execute(() => el.AddToClassList("eased")).StartingIn(20);
            }
            // (at its own depth: the layer's children are in the list's order)
            floatLayer.Insert(Mathf.Min(floats.IndexOf(f), floatLayer.childCount), f.el);
            Fronts();
            nextFill = 0f;                                               // (whoever speaks stands in the portrait's place at once)
        }

        public void CloseFloat(string id)
        {
            var f = floats.Find(x => x.spec.id == id);
            if (f == null)
                return;
            f.el?.RemoveFromHierarchy();
            floats.Remove(f);
            if (floats.Count == 0)
                floatSlots = 0;
            Fronts();
            nextFill = 0f;
        }

        void ToFront(Float f)
        {
            if (floats.Count == 0 || floats[floats.Count - 1] == f || !floats.Contains(f))
                return;
            floats.Remove(f);
            floats.Add(f);
            f.el.BringToFront();
            Fronts();
            nextFill = 0f;
        }

        void Fronts()
        {
            for (int i = 0; i < floats.Count; i++)
                floats[i].el.EnableInClassList("front", i == floats.Count - 1 && floats.Count > 1);
        }

        /// <summary>
        /// The lines of a window somebody speaks in are said a letter at a time (TextReveal: the user, 7 Oct 2026, "a
        /// Pokemon style text reveal"). Only where there is a speaker (a face, a name under the title); a window of the
        /// game's own (the lookout's list, the creation) stands whole. Lines already said stay said when the window is
        /// drawn again, and the one being said goes on from its letter.
        /// </summary>
        void Speak(Float f, ScrollView scroll)
        {
            f.reveal = null;
            var spec = f.spec;
            if (!Showtime.On || spec.content != null || (spec.face == null && string.IsNullOrEmpty(spec.who) && string.IsNullOrEmpty(spec.speaker)))
                return;
            var reveal = new TextReveal();
            foreach (var child in scroll.contentContainer.Children())
            {
                if (child is TurningLine || child.ClassListContains("frame-float-small") || child.ClassListContains("frame-float-chose"))
                    continue;
                if (child is TextElement || child is SprinkledText)
                    reveal.Add(child);
            }
            if (reveal.Count == 0)
                return;
            int same = 0;
            while (same < f.said.Count && same < reveal.Count && f.said[same] == reveal.Key(same))
                same++;
            if (same < f.said.Count)
                f.said.RemoveRange(same, f.said.Count - same);
            reveal.OneAtATime = true;                                    // (a paragraph at any one time)
            reveal.StartAt(same, same < reveal.Count && reveal.Key(same) == f.saying ? f.sayingShown : 0);
            f.reveal = reveal;
            // (a click anywhere on the face or the words)
            (scroll.parent?.parent ?? scroll).RegisterCallback<PointerDownEvent>(_ =>
            {
                if (f.reveal != null && f.reveal.Advance())
                {
                    Said(f);
                    Paging(f);
                }
            });
        }

        /// <summary>A window said one line at a time shows its choices when its last line has come; till then the line
        /// under it says how to go on.</summary>
        void Paging(Float f)
        {
            bool more = f.reveal != null && f.reveal.OneAtATime && !f.reveal.Done;
            if (f.optionsBox != null)
                f.optionsBox.style.display = more ? DisplayStyle.None : DisplayStyle.Flex;
            if (more == f.more)
                return;
            f.more = more;
            if (f.hint != null && f.hintUntil <= 0f)
                f.hint.text = StandingHint(f);
                HintShown(f);
        }

        /// <summary>
        /// THE LINES OF A WINDOW AS BOXES (Pokémon pacing): the scene's description first where the window has one
        /// (<see cref="FloatSpec.sceneFirst"/>), as the teller's; then each line cut into boxes of a sentence or two
        /// (TextReveal.Pages). Who says a box, whether it is plain or an ending, and what it points at are the line's
        /// own, carried over to each of its boxes.
        /// </summary>
        static List<(string text, bool written)> Paced(FloatSpec spec)
        {
            var boxes = new List<(string, bool)>();
            int most = TextReveal.PageChars;
            if (spec.sceneFirst)
                foreach (string box in TextReveal.Pages(spec.small ?? "", most))
                {
                    boxes.Add((box, true));
                    spec.voices[box] = new FloatVoice();                  // (the teller: no face, no name)
                }
            foreach (var (text, written) in spec.lines)
            {
                if (string.IsNullOrEmpty(text) || !written || text == spec.chose)
                {
                    boxes.Add((text, written));
                    continue;
                }
                var cut = TextReveal.Pages(text, most);
                if (cut.Count <= 1)
                {
                    boxes.Add((text, written));
                    continue;
                }
                spec.voices.TryGetValue(text, out var voice);
                spec.points.TryGetValue(text, out var cues);
                bool plain = spec.plain.Contains(text), ending = spec.ending.Contains(text);
                int at = 0;
                foreach (string box in cut)
                {
                    boxes.Add((box, true));
                    if (voice != null)
                        spec.voices[box] = voice;
                    if (plain)
                        spec.plain.Add(box);
                    if (ending)
                        spec.ending.Add(box);
                    int start = text.IndexOf(box, at, StringComparison.Ordinal);
                    if (start < 0)
                        start = at;
                    at = start + box.Length;
                    if (cues == null)
                        continue;
                    // (what the line points at as it is said: the cue standing when this box begins, and its own)
                    var own = new List<(int at, string parts)>();
                    string standing = null;
                    foreach (var cue in cues)
                    {
                        if (cue.at <= start)
                            standing = cue.parts;
                        else if (cue.at < start + box.Length)
                            own.Add((cue.at - start, cue.parts));
                    }
                    if (standing != null)
                        own.Insert(0, (0, standing));
                    if (own.Count > 0)
                        spec.points[box] = own;
                }
            }
            return boxes;
        }

        static void Said(Float f)
        {
            var r = f.reveal;
            if (r == null)
                return;
            while (f.said.Count < r.Index)
                f.said.Add(r.Key(f.said.Count));
            f.saying = r.Done ? "" : r.Key(r.Index);
            f.sayingShown = r.Shown;
        }

        VisualElement Build(Float f)
        {
            var spec = f.spec;
            // (laid out in Game/UI/Parts/Float.uxml; a choice in Option.uxml)
            var parts = FrameParts.Current;
            var el = FrameParts.Make<VisualElement>(parts != null ? parts.floatBox : null, "float");
            if (!string.IsNullOrEmpty(spec.look))
                el.AddToClassList(spec.look);
            // each a little down and to the right of the last, in the chart's free middle
            // WHAT IS SAID STANDS IN THE FOOT (the user, 7 Oct 2026, late: "why is he speaking twice, once in the middle
            // and once at the bottom; expand the bottom one for more wide text space for characters"). A window of words
            // (a card, a talk, an alert) is docked there, wide and low, over the portrait's place and the log: who
            // speaks at its left, the words in its middle, the choices at its right when they come. A window of the
            // game's own making (a list, the choosing of who you are) still floats in the middle of the world.
            bool docked = spec.content == null;
            el.EnableInClassList("docked", docked);
            if (!docked)
            {
                // (the world shows from the left side to the right edge and from the bar down to the foot: its middle)
                el.style.left = Length.Percent(61.5f + f.slot * 2.4f);
                el.style.top = Length.Percent(41.5f + f.slot * 3.2f);
            }
            el.RegisterCallback<PointerDownEvent>(_ => ToFront(f), TrickleDown.TrickleDown);

            // its bar: its name, and its cross where it may be shut (a window that floats is the one kind with a bar)
            FrameParts.In<Label>(el, "title").text = spec.title ?? "";
            var shut = FrameParts.In<Button>(el, "shut");
            if (!spec.mustAnswer)
                shut.clicked += () => Shut(f);
            else
                shut.RemoveFromHierarchy();
            // who speaks: the face, the name, what they are (each taken out where there is none)
            // (where the lines have speakers of their own the three parts stay, and change as the lines are said: Voice)
            bool voiced = spec.voices.Count > 0;
            var face = FrameParts.In<FrameFace>(el, "face");
            if (spec.face != null)
            {
                face.Who = spec.face;
                if (!string.IsNullOrEmpty(spec.faceSlot))
                    face.Slot = spec.faceSlot;
            }
            else if (!voiced)
                face.RemoveFromHierarchy();
            var who = FrameParts.In<Label>(el, "who");
            if (!string.IsNullOrEmpty(spec.who))
                who.text = spec.who;
            else if (!voiced)
                who.RemoveFromHierarchy();
            var about = FrameParts.In<Label>(el, "about");
            if (!string.IsNullOrEmpty(spec.about))
                about.text = spec.about;
            else if (!voiced)
                about.RemoveFromHierarchy();
            else
                about.style.display = DisplayStyle.None;
            if (voiced)
            {
                f.faceEl = face;
                f.whoEl = who;
                f.aboutEl = about;
                f.voiceOf = null;
            }
            var scroll = FrameParts.In<ScrollView>(el, "scroll");
            if (spec.content != null)
                scroll.Add(spec.content);
            foreach (var turn in spec.turns)
                if (turn != null && turn.pairs.Count > 0)
                    scroll.Add(new TurningLine(turn));
            bool sprinkle = spec.tongue.Length > 0 || spec.marks.Count > 0;
            // WHAT IS SAID STANDS APART FROM WHAT IS TOLD (the user, 8 Oct 2026: "What they're saying and the description
            // beneath it needs to be more clearly defined from each other its hard to tell what's actually dialogue"). A
            // line somebody says has the class "spoken" (a pale block with a bar down its left: Frame.uss); a line of the
            // teller's has "teller" and is plain and pale, as the description under the lines is.
            bool somebody = spec.face != null || !string.IsNullOrEmpty(spec.who) || !string.IsNullOrEmpty(spec.speaker);
            // POKEMON PACING (the user, 9 Oct 2026: "have it more Pokémon paced with dialogue"): where the words are said
            // (the show is on), the box holds a sentence or two at a time and the key brings the next: the scene's
            // description first, as the teller's own boxes; then each line; then what came of a choice.
            bool paced = Showtime.On && spec.content == null && somebody;
            var lines = paced ? Paced(spec) : spec.lines;
            string resultNow = paced ? "" : spec.result ?? "", smallNow = paced && spec.sceneFirst ? "" : spec.small ?? "";
            foreach (var (text, written) in lines)
            {
                if (string.IsNullOrEmpty(text))
                    continue;
                if (spec.chose.Length > 0 && text == spec.chose)
                {
                    var toldBack = Text("frame-float-chose");
                    toldBack.text = text;
                    scroll.Add(toldBack);
                    continue;
                }
                bool teller = spec.voices.TryGetValue(text, out var saidBy) && saidBy.Teller;
                bool spoken = written && somebody && !teller && !spec.plain.Contains(text);
                bool ending = spec.ending.Contains(text);
                if (sprinkle && written)
                {
                    // (a line with words of the speaker's tongue in it: each set apart, its meaning on hover)
                    var runs = TongueBook.Current.Mark(text, spec.tongue, spec.marks);
                    if (runs.Exists(r => r.word != null))
                    {
                        var sprinkled = new SprinkledText(runs, "frame-float-sprinkle");
                        sprinkled.EnableInClassList("spoken", spoken);
                        sprinkled.EnableInClassList("teller", teller);
                        sprinkled.EnableInClassList("ending", ending);
                        scroll.Add(sprinkled);
                        continue;
                    }
                }
                var line = Text("frame-float-line");
                line.text = text;
                line.EnableInClassList("unwritten", !written);
                line.EnableInClassList("spoken", spoken);
                line.EnableInClassList("teller", teller);
                line.EnableInClassList("ending", ending);
                scroll.Add(line);
            }
            if (paced)
            {
                // (what came of it, in facts: boxes too, in the result's own dress)
                foreach (string box in TextReveal.Pages(spec.result ?? "", TextReveal.PageChars))
                {
                    var result = Text("frame-float-result");
                    result.text = box;
                    scroll.Add(result);
                }
            }
            else if (resultNow.Length > 0)
            {
                var result = Text("frame-float-result");
                result.text = resultNow;
                scroll.Add(result);
            }
            if (smallNow.Length > 0)
            {
                var small = Text("frame-float-small");
                small.text = smallNow;
                scroll.Add(small);
            }
            // (the words and the choices in one body: one over the other in a window that floats, side by side in the foot)
            Speak(f, scroll);
            el.EnableInClassList("paged", f.reveal != null && f.reveal.OneAtATime);
            // (the mark that the box waits for the key: Pokémon's own, at the foot of the words. It is a part of the box's
            // own UI file, named "more"; where nothing is said a box at a time it is taken out of this copy)
            f.moreMark = FrameParts.In<FrameGlyph>(el, "more");
            if (f.reveal != null && f.reveal.OneAtATime)
                f.moreMark.style.visibility = Visibility.Hidden;
            else
            {
                f.moreMark.RemoveFromHierarchy();
                f.moreMark = null;
            }
            f.optionsBox = null;

            var list = FrameParts.In<VisualElement>(el, "options");
            if (spec.options.Count > 0)
            {
                for (int i = 0; i < spec.options.Count; i++)
                {
                    var o = spec.options[i];
                    int index = i;
                    var b = FrameParts.Make<Button>(parts != null ? parts.option : null, "option");
                    b.clicked += () => Pick(f, index);
                    b.SetEnabled(o.enabled);
                    FrameParts.In<Label>(b, "number").text = (i + 1) + ".";
                    // (a choice whose own words end with its odds in brackets, "... (doubtful)", is not given them twice:
                    // the odds stand at the line's right end)
                    string said = o.label ?? "";
                    if (!string.IsNullOrEmpty(o.odds) && said.EndsWith(" (" + o.odds + ")", StringComparison.Ordinal))
                        said = said.Substring(0, said.Length - o.odds.Length - 3);
                    FrameParts.In<Label>(b, "label").text = (string.IsNullOrEmpty(o.tag) ? "" : "[" + o.tag + "] ") + said;
                    var rule = FrameParts.In<Label>(b, "rule");
                    if (!o.enabled && !string.IsNullOrEmpty(o.rule))
                        rule.text = o.rule;
                    else
                        rule.RemoveFromHierarchy();
                    var odds = FrameParts.In<Label>(b, "odds");
                    if (!string.IsNullOrEmpty(o.odds))
                        odds.text = o.odds;
                    else
                        odds.RemoveFromHierarchy();
                    var key = FrameParts.In<Label>(b, "key");
                    if (o.isDefault && o.enabled)
                        key.text = "Enter";
                    else
                        key.RemoveFromHierarchy();
                    foreach (var child in b.Children())
                        child.pickingMode = PickingMode.Ignore;
                    list.Add(b);
                }
                f.optionsBox = list;
            }
            else
                list.RemoveFromHierarchy();
            f.hint = FrameParts.In<Label>(el, "hint");
            f.more = f.reveal != null && f.reveal.OneAtATime && !f.reveal.Done;
            f.hint.text = StandingHint(f);
            HintShown(f);
            if (f.optionsBox != null && f.more)
                f.optionsBox.style.display = DisplayStyle.None;
            return el;
        }

        static void HintShown(Float f)
        {
            if (f == null || f.hint == null)
                return;
            f.hint.style.display = string.IsNullOrEmpty(f.hint.text) ? DisplayStyle.None : DisplayStyle.Flex;
            // (a card left to wait: its line is a row of its own under the window, so it covers nothing. The line that
            // says for a moment why a choice cannot be taken lies over the window's foot, as it did)
            f.hint.parent?.EnableInClassList("waits", f.released && f.spec.waits != null);
        }

        static string StandingHint(Float f)
        {
            // Nothing stands under a window any more (the user, 7 Oct 2026, late, of "Enter, or a click: go on.": "remove
            // it, you don't have to explain everything like this"). What a key does is in the tips and in the book of
            // rules; the line is kept for what has to be said once: why a choice cannot be taken.
            // And (8 Oct 2026, rulings 10 and 12) for a card the player has let the clock run on: how long it will wait
            // yet, and what it then does by itself. Nothing while the clock is held: no time passes then.
            if (f != null && f.released && f.spec.waits != null)
                return f.spec.waits() ?? "";
            return "";
        }

        void Hint(Float f, string text)
        {
            if (f.hint == null)
                return;
            f.hint.text = text ?? "";
            HintShown(f);
            f.hintUntil = Time.realtimeSinceStartup + 4f;
        }

        void Pick(Float f, int index)
        {
            if (index < 0 || index >= f.spec.options.Count)
                return;
            var o = f.spec.options[index];
            if (!o.enabled)
            {
                Hint(f, string.IsNullOrEmpty(o.rule) ? "Not now." : o.rule);
                return;
            }
            o.pick?.Invoke();
        }

        void Shut(Float f)
        {
            if (f.spec.mustAnswer)
            {
                Hint(f, "Choose one first.");
                return;
            }
            if (f.spec.shut != null)
                f.spec.shut();
            else
                CloseFloat(f.spec.id);
        }

        // ------------------------------------------------------------------ the frame's hooks

        /// <summary>The layer the windows float in begins under the top bar: a window is placed and sized by shares of
        /// it, so a tall one never lies over the bar, also where a small screen makes the bar two lines high.</summary>
        partial void FloatsTop(float y)
        {
            if (floatLayer != null)
                floatLayer.style.top = y;
        }

        partial void FloatsBuild()
        {
            floats.Clear();
            floatSlots = 0;
            floatLayer = Part<VisualElement>("floats");                    // (the layout file, Frame.uxml: "floats")
            pointer = Part<VisualElement>("pointer");
            pointer.pickingMode = PickingMode.Ignore;
            pointer.style.display = DisplayStyle.None;
            PointedAt = "";
            cardShown = null;
            talkVersion = -1;
        }

        partial void FloatsEnable()
        {
            // only a card or a talk that needs an answer holds the clock, and the player may set it running again at once
            // (Space, a speed button): what was holding lets go and stays open
            PanelClock.Holds = () => FloatHoldsClock;
            PanelClock.LetGo = () =>
            {
                foreach (var f in floats)
                    if (f.spec.holdsClock && !f.released)
                    {
                        f.released = true;
                        f.hintUntil = 0f;
                        if (f.hint != null)
                            f.hint.text = StandingHint(f);
                            HintShown(f);
                    }
            };
            // a card waits only for the title, the pause menu and the chest: it may open over the port's own business
            // (and for a happening of the events table that is up and wants its answer: one thing at a time)
            BoatWatch.GameOver -= LostFloat;
            BoatWatch.GameOver += LostFloat;
            EncounterRunner.Blocked = () =>
                (MainMenu.Current != null && (MainMenu.Current.Open || MainMenu.Current.OnTitle)) ||
                Showtime.HoldsCards ||                                   // (the opening, and until Ravn has had his say)
                (MapView.Current != null && MapView.Current.ChestOpen) ||
                GameEvents.Current != null;
        }

        partial void FloatsDisable()
        {
            EncounterRunner.Blocked = null;
            BoatWatch.GameOver -= LostFloat;
            if (hooked != null)
            {
                hooked.Chose -= CardChose;
                hooked.TookItself -= CardTookItself;
            }
            hooked = null;
            if (oldCardPanelOff && oldCardPanel != null)
                oldCardPanel.enabled = true;
            oldCardPanelOff = false;
            floats.Clear();
        }

        partial void FloatsUpdate()
        {
            if (!oldCardPanelOff && oldCardLooks < 8 && Time.realtimeSinceStartup >= nextOldCardLook)
            {
                // the card's old place (drawn with IMGUI over the scene) stands down: the card floats here now
                // (looked for a few times as the scene comes up, not every frame for ever: a scene may have none)
                oldCardLooks++;
                nextOldCardLook = Time.realtimeSinceStartup + 0.5f;
                oldCardPanel = oldCardPanel != null ? oldCardPanel : FindAnyObjectByType<EncounterPanel>();
                if (oldCardPanel != null)
                {
                    oldCardPanel.enabled = false;
                    oldCardPanelOff = true;
                }
            }
            CardFloat();
            OrderAsk.Tick(this);                                         // (an order a card offered is asked when the card has closed)
            if (!Boat.IsLost && FloatOpen(LostId))
                CloseFloat(LostId);                                      // (a saved game was loaded: she swims again)
            TalkFloat();
            AlertFloats();
            PeopleLeftBehind();
            TurningLine.TickAll();                                       // (a line turning from a tongue into English goes by the wall clock)
            foreach (var f in floats)
                if (f.reveal != null)
                {
                    f.reveal.Tick(Time.unscaledDeltaTime);
                    Said(f);
                    Paging(f);
                    // (the mark blinks while a box stands whole and another is to come)
                    if (f.moreMark != null)
                        f.moreMark.style.visibility = f.reveal.Waiting && Time.unscaledTime % 0.9f < 0.6f ? Visibility.Visible : Visibility.Hidden;
                }
            PointerUpdate();
            foreach (var f in floats)
                Voice(f);
            float now = Time.realtimeSinceStartup;
            foreach (var f in floats)
                if (f.hintUntil > 0f && now > f.hintUntil)
                {
                    f.hintUntil = 0f;
                    f.hint.text = StandingHint(f);
                    HintShown(f);
                }
                else if (f.hintUntil <= 0f && f.released && f.spec.waits != null && f.hint != null)
                {
                    // (a card left to wait: its hours run down while the clock runs)
                    string waits = StandingHint(f);
                    if (f.hint.text != waits)
                    {
                        f.hint.text = waits;
                        HintShown(f);
                    }
                }
        }

        /// <summary>A talk with someone of a port ends when she no longer lies there (cast off with the window still up,
        /// she would have gone on talking with Ravn of Dorestat half way to Lundenwic). A moment's grace: the port's own
        /// business may be off the panel for a frame between two of its steps.</summary>
        void PeopleLeftBehind()
        {
            bool any = false;
            foreach (var f in floats)
                any |= f.spec.id.StartsWith(PersonPrefix, StringComparison.Ordinal);
            if (!any)
            {
                peopleAwaySince = -1f;
                return;
            }
            var here = FrameGame.PortHere;
            bool away = false;
            foreach (var f in floats)
            {
                if (!f.spec.id.StartsWith(PersonPrefix, StringComparison.Ordinal))
                    continue;
                var who = PlaceBook.Current.Person(f.spec.id.Substring(PersonPrefix.Length));
                away |= here == null || who == null || who.port != here.id;
            }
            if (!away)
            {
                peopleAwaySince = -1f;
                return;
            }
            float now = Time.realtimeSinceStartup;
            if (peopleAwaySince < 0f)
                peopleAwaySince = now;
            if (now - peopleAwaySince < 1f)
                return;
            peopleAwaySince = -1f;
            for (int i = floats.Count - 1; i >= 0; i--)
            {
                string id = floats[i].spec.id;
                if (!id.StartsWith(PersonPrefix, StringComparison.Ordinal))
                    continue;
                var who = PlaceBook.Current.Person(id.Substring(PersonPrefix.Length));
                if (here == null || who == null || who.port != here.id)
                    CloseFloat(id);
            }
        }

        const string PersonPrefix = "person:";
        float peopleAwaySince = -1f, nextOldCardLook;
        int oldCardLooks;

        // ------------------------------------------------------------------ the saved game

        /// <summary>A panel's full window is the floating window "full.&lt;panel id&gt;" (the ruling of 7 October: every
        /// panel a brief face and a full window; the brief faces' builder opens them).</summary>
        public const string FullPrefix = "full.";

        partial void FloatsCapture()
        {
            if (FrameSave.OpenFull == null)
                return;                                                   // (full windows are not built: what a save had is kept as it is)
            FrameSave.Full.Clear();
            foreach (var f in floats)                                     // back to front
                if (f.spec.id.StartsWith(FullPrefix, StringComparison.Ordinal))
                    FrameSave.Full.Add(new FrameSave.FullWindow
                    {
                        panel = f.spec.id.Substring(FullPrefix.Length),
                        left = f.el.style.left.value.value, top = f.el.style.top.value.value,   // (percent, as Build sets them)
                    });
        }

        /// <summary>A game was loaded under the frame. The windows over the chart that were of the game before go: a talk
        /// with someone of a port, the lookout's, a wait's, a happening's. A card and a talk of the graphs follow the
        /// loaded game by themselves (CardFloat, TalkFloat). The full windows the loaded game had open come back.</summary>
        partial void FloatsLoaded()
        {
            for (int i = floats.Count - 1; i >= 0; i--)
            {
                string id = floats[i].spec.id;
                if (id != CardId && id != TalkId && id != SelfId)
                    CloseFloat(id);
            }
            eventUp = null;
            alertsAnswered.Clear();
            alertsTold.Clear();
            seaLogSeen = -1;                                              // (the sea log is of the game before: not told again)
            if (FrameSave.OpenFull == null)
                return;
            foreach (var w in FrameSave.Full.ToArray())
            {
                if (FramePanels.Get(w.panel) == null)
                    continue;                                             // (a panel that is no longer there)
                var el = FrameSave.OpenFull(w.panel);
                if (el == null || w.left < 0f || w.top < 0f)
                    continue;
                el.style.left = Length.Percent(w.left);
                el.style.top = Length.Percent(w.top);
            }
        }

        partial void FloatsKeys(Keyboard kb, ref bool taken)
        {
            if (floats.Count == 0)
                return;
            var front = floats[floats.Count - 1];
            for (int i = 0; i < 9; i++)
                if (kb[Key.Digit1 + i].wasPressedThisFrame || kb[Key.Numpad1 + i].wasPressedThisFrame)
                {
                    taken = true;
                    if (i < front.spec.options.Count && !front.more)     // (its choices are not up while there is more to be said)
                        Pick(front, i);
                    return;
                }
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            {
                taken = true;
                if (front.reveal != null && !front.reveal.Done)
                {
                    front.reveal.Advance();                              // (Enter while somebody speaks: the line is finished, the next begins)
                    Said(front);
                    Paging(front);
                    return;
                }
                int enter = front.spec.options.FindIndex(o => o.isDefault && o.enabled);
                if (enter < 0 && front.spec.options.Count == 1)
                    enter = 0;
                if (enter >= 0)
                    Pick(front, enter);
            }
            else if (kb.escapeKey.wasPressedThisFrame)
            {
                taken = true;
                Shut(front);
            }
        }

        partial void FloatsWho(ref WhoIs now)
        {
            // whoever is speaking stands in the portrait's place while they speak: the front window with a speaker
            for (int i = floats.Count - 1; i >= 0; i--)
            {
                var s = floats[i].spec;
                if (string.IsNullOrEmpty(s.speaker) || s.content == null)
                    continue;                                             // (a window of words is in the foot itself, over the portrait's place: said once)
                string say = "";
                foreach (var (text, written) in s.lines)
                    if (written && !string.IsNullOrEmpty(text) && string.IsNullOrEmpty(s.result))   // (after the choice the line is the player's own, not theirs)
                    {
                        say = text.Length > 110 ? text.Substring(0, 107).TrimEnd() + "..." : text;
                        break;
                    }
                now = new WhoIs { name = s.speaker, line1 = s.speakerLine, line2 = "", line3 = "", say = say, face = s.face ?? s.speaker };
                return;
            }
        }

        // ------------------------------------------------------------------ the last page

        public const string LostId = "lost";

        /// <summary>
        /// THE LAST PAGE (the user, 2 Oct 2026: a wreck is the end of the game; Design/fixes/risk_is_real.md 6.4). She is
        /// lost: one window says how, with the last lines of the log (they tell the signs, the order and the wounds in
        /// their order), and two ways on: the last saved game, or the title. The clock stands and nothing is saved over
        /// the games from before. PLACEHOLDER WORDS, the user's to write.
        /// </summary>
        public static FloatSpec LostSpec(string how, IList<string> log, bool anySave, Action load, Action title)
        {
            var spec = new FloatSpec { id = LostId, title = "She is lost", mustAnswer = true, holdsClock = true };
            spec.lines.Add((string.IsNullOrEmpty(how) ? "She is lost." : how, true));
            if (log != null && log.Count > 0)
            {
                spec.lines.Add(("How it went, to the last:", true));
                foreach (string line in log)
                    if (!string.IsNullOrEmpty(line) && line != how)
                        spec.lines.Add((line, true));
            }
            spec.options.Add(new FloatOption
            {
                label = "Load the last saved game", enabled = anySave, rule = anySave ? "" : "No game was saved.", isDefault = anySave, pick = load,
            });
            spec.options.Add(new FloatOption { label = "To the title", pick = title });
            return spec;
        }

        void LostFloat(string how)
        {
            var last = new List<string>();
            for (int i = Math.Max(0, FrameLog.Lines.Count - 8); i < FrameLog.Lines.Count; i++)
                last.Add(FrameLog.Lines[i].text);
            OpenFloat(LostSpec(how, last, SaveGame.Slots().Count > 0, LoadLast, ToTitle));
        }

        /// <summary>The newest saved game is loaded (the last page's first way on). The page shuts when she swims again.</summary>
        void LoadLast()
        {
            var saves = SaveGame.Slots();
            if (saves.Count == 0)
                return;
            string fact = SaveGame.Load(saves[0].slot);
            FrameLog.Add(fact);
        }

        void ToTitle()
        {
            CloseFloat(LostId);
            if (MainMenu.Current != null)
                MainMenu.Current.ShowTitle();
        }

        // ------------------------------------------------------------------ the card in play

        const string CardId = "card";

        void CardFloat()
        {
            var d = EncounterRunner.Director;
            if (d != hooked)
            {
                if (hooked != null)
                {
                    hooked.Chose -= CardChose;
                    hooked.TookItself -= CardTookItself;
                }
                hooked = d;
                if (d != null)
                {
                    d.Chose += CardChose;
                    d.TookItself += CardTookItself;
                }
            }
            var s = d != null ? d.Current : null;
            if (s == null)
            {
                if (cardShown != null)
                {
                    cardShown = null;
                    CloseFloat(CardId);
                }
                return;
            }
            if (s == cardShown && s.Chosen == cardChosen)
                return;
            bool fresh = s != cardShown;
            cardShown = s;
            cardChosen = s.Chosen;
            if (fresh)
            {
                // (a person of the places' files met on a card is met, as when he is spoken with on the quay: his page
                // of the Book opens. The play-through of 9 Oct 2026 ended a season with none of 502 open.)
                var playing = KapmanGame.Current;
                foreach (var who in s.People)
                    if (who != null && PlaceBook.Current.Person(who.id) != null)
                        Self.MarkMet(who.id, playing != null ? playing.Day : 0.0);
            }
            if (fresh)
            {
                // (a new card holds the clock anew, though the last one, in this same window, had been let wait)
                var was = floats.Find(f => f.spec.id == CardId);
                if (was != null)
                    was.released = false;
            }
            OpenFloat(CardSpec(d, s));
            if (fresh)
                ToFront(floats.Find(f => f.spec.id == CardId));
        }

        /// <summary>Who somebody is, in one line under their name (the user, 7 Oct 2026, late: "add their personality type /
        /// stats"): their people, their faith, and their nature where the places' files give them one (its first
        /// clause). People have no figures of their own yet: when they do, they stand here.</summary>
        public static string About(Npc who)
        {
            if (who == null)
                return "";
            var parts = new List<string>();
            string people = SelfBook.Current.People(who.people)?.name ?? Cap((who.people ?? "").Replace('_', ' '));
            string faith = SelfBook.Current.Faith(who.faith)?.name ?? Cap((who.faith ?? "").Replace('_', ' '));
            if (people.Length > 0)
                parts.Add(people);
            if (faith.Length > 0)
                parts.Add(faith);
            string trait = PlaceBook.Current.Person(who.id)?.trait ?? "";
            int cut = trait.IndexOfAny(new[] { ';', '.', ':' });
            if (cut > 0)
                trait = trait.Substring(0, cut);
            if (trait.Length > 0)
                parts.Add(trait);
            return string.Join(" \u00b7 ", parts);
        }

        /// <summary>A card as a floating window: who speaks, the lines, the choices with their odds as a word; after the
        /// choice, what came of it and the way on.</summary>
        public static FloatSpec CardSpec(EncounterDirector d, EncounterSession s)
        {
            // (THE CARD HOLDS THE CLOCK UNTIL IT IS GONE ON FROM: what came of it too. Until 9 Oct 2026 the clock ran
            // again from the choice, and at the fastest clock a day passed in the four seconds its end takes to read:
            // the play-through's evening song opened at 22:00 and was done at 01:45. The player may still set the
            // clock running under it, as under any card. My reading of the ruling of 7 Oct; the user's to overrule.)
            var spec = new FloatSpec { id = CardId, title = s.Title, holdsClock = true, mustAnswer = !s.Chosen };
            Npc first = null;
            var others = new List<string>();
            foreach (var who in s.People)
            {
                if (first == null)
                    first = who;
                else
                    others.Add(s.Speaker(who));
            }
            if (first != null)
            {
                spec.who = s.Speaker(first);
                spec.face = first.id;
                spec.faceSlot = FrameArt.OfSpeaker(first.id, first.role);
                spec.speaker = first.name.Length > 0 ? first.name : spec.who;
                spec.about = About(first);
                spec.speakerLine = first.name.Length > 0 && spec.who.StartsWith(first.name + ", ", StringComparison.Ordinal)
                    ? Cap(spec.who.Substring(first.name.Length + 2)) : "";
                if (others.Count > 0)
                    spec.who += "; with " + string.Join("; ", others);
            }
            else
                spec.face = "";                                             // (a happening with nobody to speak: an empty place)
            if (!s.Chosen)
            {
                foreach (var l in s.lines)
                {
                    // (a line that names a real line of the tongues file comes up as that line, in its own tongue, and
                    // turns into English; a name the file does not have, or a line not yet spoken that year, is passed over)
                    var real = l.turns.Length > 0 ? TongueBook.Current.Line(l.turns, s.world.year) : null;
                    if (real != null)
                        spec.turns.Add(FloatTurn.Of(real));
                    else if (l.turns.Length > 0)
                        ContentFiles.Warn("turns:" + l.turns, $"the card {s.card.id} names the line '{l.turns}' to turn from, and story/tongues_793.json has no such line for {s.world.year}: shown in English only");
                    spec.lines.Add((l.text, l.written));
                    // (who says it: one of the card's people, or the teller, who has no face)
                    spec.voices[l.text] = l.who == null ? new FloatVoice()
                                                        : new FloatVoice { who = s.Speaker(l.who), about = About(l.who), face = l.who.id, faceSlot = FrameArt.OfSpeaker(l.who.id, l.who.role) };
                    if (spec.tongue.Length == 0 && l.tongue.Length > 0)
                        spec.tongue = l.tongue;                           // (the first speaker's tongue: its words in the lines are set apart)
                }
                spec.small = s.Intention ?? "";
                spec.sceneFirst = true;                                  // (the scene is told first, a box at a time, where the talk is paced)
                for (int i = 0; i < s.options.Count; i++)
                {
                    var o = s.options[i];
                    int index = i;
                    spec.options.Add(new FloatOption
                    {
                        label = o.label, tag = o.tag, odds = o.odds, enabled = o.enabled, rule = o.rule, isDefault = o.isDefault,
                        pick = () => d.Choose(index),
                    });
                }
                spec.waits = () => WaitLine(d, s);
            }
            else
            {
                string chose = "";
                foreach (var o in s.options)
                    if (o.def.key == s.result.option)
                        chose = o.Tagged;
                if (chose.Length > 0)
                {
                    spec.chose = "\u203a " + chose;                      // (the player's own choice, told back: small, not speech)
                    spec.lines.Add((spec.chose, true));
                    spec.plain.Add(spec.chose);
                }
                // WHAT CAME OF IT, AS IT IS WRITTEN (8 Oct 2026: the script's two thousand ends of choices were never
                // shown). The paragraph first; then the effects' facts in the box. Where no end is written, the box
                // says what you did and the facts, as before.
                string end = s.result.written ?? "";
                if (end.Length > 0)
                {
                    spec.lines.Add((end, true));
                    spec.plain.Add(end);
                    spec.ending.Add(end);
                }
                spec.result = end.Length > 0 ? s.result.Facts : s.result.Text;
                spec.options.Add(new FloatOption { label = "Go on", isDefault = true, pick = () => d.Continue() });
                spec.shut = () => d.Continue();
            }
            return spec;
        }

        void CardChose(EncounterSession s, ChoiceResult r)
        {
            var d = EncounterRunner.Director;
            if (d != null && d.TakingItself)
                return;                                                  // (the card answered itself: CardTookItself writes it)
            if (s != null && r != null && r.ok)
                FrameLog.Add(r.Text);                                    // (what you did and what came of it, in sentences: no title and colon)
        }

        /// <summary>A card left unanswered took an option by itself (rulings 10 and 12 of 7 Oct 2026: "and says so in the
        /// log"): its time ran out with the clock running, or she cast off from its port.</summary>
        void CardTookItself(EncounterSession s, ChoiceResult r, bool castOff)
        {
            if (s != null && r != null)
                FrameLog.Add(TookItselfLine(s, r, castOff));
        }

        /// <summary>The log's line for it. PLACEHOLDER WORDS, the user's to write.</summary>
        public static string TookItselfLine(EncounterSession s, ChoiceResult r, bool castOff)
        {
            string label = "";
            foreach (var o in s.options)
                if (o.def.key == r.option)
                    label = o.label;
            string how = castOff ? "You cast off and left \u201c" + s.Title + "\u201d unanswered" : "You gave \u201c" + s.Title + "\u201d no answer";
            // (where the card has no line of its own for the choice, what was told is only the choice's words again)
            string came = r.told == LogProse.Answered(label) ? r.Facts : r.Text;
            return (how + ", so it went this way: " + label.TrimEnd('.', ' ') + ". " + came).Trim();
        }

        /// <summary>
        /// THE LINE UNDER A CARD LEFT TO WAIT (rulings 10 and 12): "It waits 6 days and 4 hours more. Then: Declare the
        /// cargo." A card that holds the ship says so first. The option named is the one it would take as things stand
        /// now. PLACEHOLDER WORDS, the user's to write.
        /// </summary>
        public static string WaitLine(EncounterDirector d, EncounterSession s)
        {
            if (d == null || s == null || d.Current != s || s.Chosen)
                return "";
            int own = d.OwnOption;
            string held = d.HoldsShip ? "She is held here. " : "";
            if (own < 0)
                return held + "It waits for your answer.";
            string wait = d.HoursLeft > 0.0 ? "It waits " + Span(d.HoursLeft) + " more. Then: " : "It waits no longer: ";
            return held + wait + s.options[own].label.TrimEnd('.', ' ') + ".";
        }

        /// <summary>A span of game hours in words, rounded up to the hour: "7 days", "6 days and 5 hours", "1 hour".</summary>
        public static string Span(double hours)
        {
            int h = (int)Math.Ceiling(Math.Max(0.0, hours) - 1e-9);
            int days = h / 24, rest = h % 24;
            string a = days > 0 ? days + (days == 1 ? " day" : " days") : "";
            string b = rest > 0 || days == 0 ? rest + (rest == 1 ? " hour" : " hours") : "";
            return a.Length > 0 && b.Length > 0 ? a + " and " + b : a + b;
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        // ------------------------------------------------------------------ a talk (the text panel, but the port's own menu)

        const string TalkId = "talk";

        void TalkFloat()
        {
            var text = TextPanel.Current;
            bool talk = text != null && text.Open && text.Key != TextPanel.PortMenuKey;
            if (!talk)
            {
                if (talkVersion >= 0)
                {
                    talkVersion = -1;
                    talkLine = null;
                    CloseFloat(TalkId);
                }
                return;
            }
            if (text.Version == talkVersion)
                return;
            bool fresh = talkVersion < 0;
            talkVersion = text.Version;
            // (a line that runs, the chase's numbers twice a second: changed in place. Building the window again each
            // time would lose a click that fell across the change)
            string shape = TalkShape(text);
            if (!fresh && shape == talkShape && talkLine != null && talkLine.panel != null)
            {
                talkLine.text = TalkLine(text);
                return;
            }
            bool newTalk = fresh || text.Key != talkKey;
            talkShape = shape;
            talkKey = text.Key;
            var spec = new FloatSpec { id = TalkId, title = text.TitleText, mustAnswer = true, holdsClock = true };
            if (!string.IsNullOrEmpty(text.StoryText))
                spec.lines.Add((text.StoryText, true));
            // (a ship met: her facts begin with what she is, which is the window's title already: said once)
            string facts = TalkLine(text);
            if (facts.Length > 0)
                spec.lines.Add((facts, true));
            spec.result = text.OutcomeText;
            int back = -1;
            for (int i = 0; i < text.Options.Count; i++)
            {
                var (key, label, tip) = text.Options[i];
                int index = i;
                if (key == "port.back" || text.Options.Count == 1)
                    back = i;
                spec.options.Add(new FloatOption { label = label, isDefault = text.Options.Count == 1, pick = () => text.Pick(index) });
            }
            if (back >= 0)
            {
                // a talk with a way back may be shut: shutting it takes that way
                int way = back;
                spec.mustAnswer = false;
                spec.holdsClock = false;                                   // (it needs no answer: the clock runs)
                spec.shut = () => text.Pick(way);
            }
            else if (text.Options.Count == 0)
            {
                // (nothing to choose yet, or only something told: it must not hold the clock with no way to answer)
                spec.mustAnswer = false;
                spec.holdsClock = false;
            }
            else if (text.Key == "encounter.raiders.running")
                spec.holdsClock = false;                                   // (a chase is live: it runs while she runs; the choices end it)
            else if (GameSpeed.HeldFor("alongside"))
            {
                // (two ships lying together: the game holds the clock itself until they part, and Space does not let it
                // run. The window must not say that it does; the talk's own first line says that time stands still.)
                spec.holdsClock = false;
            }
            OpenFloat(spec);
            var shown = floats.Find(f => f.spec.id == TalkId);
            if (shown != null && newTalk)
            {
                // (another talk in the same window: its own hold on the clock, whatever the player did under the last)
                shown.released = false;
                shown.hint.text = StandingHint(shown);
                HintShown(shown);
            }
            talkLine = null;
            if (shown != null && facts.Length > 0)
                shown.el.Query<Label>(className: "frame-float-line").ForEach(l =>
                {
                    if (l.text == facts)
                        talkLine = l;
                });
            if (fresh)
                ToFront(shown);
        }

        /// <summary>The line under a talk's title, as the window shows it: the panel's own, without the title said again at
        /// its head ("A coaster of Dommoc. 7 km to the north." under the title "A coaster of Dommoc").</summary>
        static string TalkLine(TextPanel text)
        {
            string line = text.IntentionText ?? "";
            string title = text.TitleText ?? "";
            if (title.Length > 0 && line.StartsWith(title + ". ", StringComparison.Ordinal))
                line = line.Substring(title.Length + 2);
            return line;
        }

        /// <summary>What a talk's window is built from, but the line under its title: when only that line has changed the
        /// window is not built again.</summary>
        static string TalkShape(TextPanel text)
        {
            var b = new System.Text.StringBuilder();
            b.Append(text.Key).Append('|').Append(text.TitleText).Append('|').Append(text.StoryText).Append('|').Append(text.OutcomeText);
            foreach (var (key, label, _) in text.Options)
                b.Append('|').Append(key).Append('=').Append(label);
            return b.ToString();
        }

        // ------------------------------------------------------------------ alerts

        /// <summary>
        /// What needs the player at sea (Alerts: raised while the map is open, and on the chart it always is). An urgent one
        /// (raiders made out, a guard boat, aground) holds the game and floats until it is answered; the others are said in
        /// the log, once. An urgent one answered and raised again the same day is let go without a word, so the game
        /// cannot be held in a loop by something that is still in sight.
        /// </summary>
        void AlertFloats()
        {
            var game = KapmanGame.Current;
            double today = game != null ? game.Day : 0.0;
            Alert again = null;
            foreach (var a in Alerts.Open)
            {
                if (!a.urgent)
                {
                    // (a sail made out is said by the lookout's own log already: FrameRoot.Rack.cs, LookoutLog)
                    if (alertsTold.Add(a.id) && a.kind != "ship")
                        FrameLog.Add(a.text.Replace(" Go to the ship to hail her.", ""));
                    continue;
                }
                string id = "alert:" + a.id;
                if (alertsAnswered.TryGetValue(a.id, out double day) && today - day < 1.0)
                {
                    again = a;                                             // (answered a little while ago: not asked again today)
                    continue;
                }
                if (FloatOpen(id))
                    continue;
                var alert = a;
                var spec = new FloatSpec { id = id, title = AlertTitle(a.kind), mustAnswer = true };
                spec.lines.Add((a.text, true));
                spec.options.Add(new FloatOption
                {
                    label = "Go on", isDefault = true,
                    pick = () =>
                    {
                        alertsAnswered[alert.id] = KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0;
                        CloseFloat(id);
                        Alerts.Answer(alert);
                        FrameLog.Add(alert.text);
                    },
                });
                OpenFloat(spec);
            }
            if (again != null)
                Alerts.Answer(again);
            // a float whose alert is over (the ship sailed out of sight) goes
            for (int i = floats.Count - 1; i >= 0; i--)
            {
                string id = floats[i].spec.id;
                if (!id.StartsWith("alert:", StringComparison.Ordinal))
                    continue;
                bool open = false;
                foreach (var a in Alerts.Open)
                    open |= "alert:" + a.id == id;
                if (!open)
                    CloseFloat(id);
            }
        }

        static string AlertTitle(string kind)
        {
            switch (kind)
            {
                case "raiders": return "Raiders";
                case "law": return "The king's men";
                case "aground": return "Aground";
                default: return "The lookout";
            }
        }
    }
}
