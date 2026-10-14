using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// NOTES WHILE PLAYING (the user, 7 Oct 2026: "create a mode where I can edit and note changes to be made on the go
    /// while playing"). [F2] holds the clock and turns the pointer into a picker: the thing under it is ringed, and a
    /// click on it opens a small slip with two boxes, what should change, and (where the thing has words) the words it
    /// should say instead. [F2] again in the picker takes the whole screen. Saving writes the note, with what was
    /// clicked, where and when in the game, and a picture of the screen, to the project's Notes folder:
    ///   Notes/play_notes.md      to read
    ///   Notes/play_notes.jsonl   the same, a line a note, for whoever works through them
    ///   Notes/shots/note_0001.png
    /// New words are also put on the thing at once, so the change can be looked at; that lasts until the game next
    /// redraws it. The lasting change is made from the notes.
    /// </summary>
    public sealed class PlayNotes : MonoBehaviour
    {
        public static PlayNotes Current { get; private set; }

        /// <summary>The clock's hold while the picker or the slip is up.</summary>
        public const string Why = "notes";

        /// <summary>Another folder for the notes (the tests: a folder of their own). Null: the project's Notes folder,
        /// beside Assets (in a built game, the player's data folder).</summary>
        public static string FolderOverride;

        public static string Folder =>
            FolderOverride ?? (Application.isEditor ? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Notes"))
                                                    : Path.Combine(Application.persistentDataPath, "Notes"));

        public bool Picking { get; private set; }
        public bool Writing { get; private set; }
        /// <summary>The picker or the slip is up, or went this frame: the frame and the menu leave the keys alone (Esc
        /// that leaves the notes must not open the menu as well).</summary>
        public static bool Busy => Current != null && (Current.Picking || Current.Writing || Time.frameCount <= quietUntil);
        static int quietUntil = -1;
        /// <summary>Notes written down so far (counted from the file, so the numbers go on from one day to the next).</summary>
        public int Count { get; private set; }
        /// <summary>The slip's two boxes while it is up (null else): what should change, and the new words (null when the
        /// thing has none).</summary>
        public TextField NoteBox { get; private set; }
        public TextField WordsBox { get; private set; }
        /// <summary>What the slip up now is about, in words ("the words \"Cast off\" in the Voyage window").</summary>
        public string About { get; private set; } = "";

        VisualElement veil, mark, hint, slip;
        VisualElement target;
        TextElement targetWords;
        string targetText = "", targetPath = "", targetWindow = "", shot = "";
        Rect targetBox;
        Vector2 at;
        bool counted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = null;
            quietUntil = -1;
            SceneManager.sceneLoaded -= Loaded;
            SceneManager.sceneLoaded += Loaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void First() => TryStart();

        static void Loaded(Scene scene, LoadSceneMode mode) => TryStart();

        // AN OBJECT OF THE SCENE (KAPMAN, step 1; the user, 7 Oct 2026: "make it like people usually make it, not just all
        // in script, build it physically"). It stands in the Game scene under SYSTEMS, to be seen and set in the
        // Inspector. Where a scene has none (a test's own scene) it still makes itself, as it always did.
        void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }
            Current = this;
        }

        static void TryStart()
        {
            if (Current != null || FrameRoot.Current == null)
                return;
            new GameObject("Play notes").AddComponent<PlayNotes>();
        }

        void OnDestroy()
        {
            Leave();
            if (Current == this)
                Current = null;
        }

        void Update()
        {
            var frame = FrameRoot.Current;
            if (frame == null || frame.Root == null)
            {
                if (Picking || Writing)
                    Leave();
                return;
            }
            var kb = Keyboard.current;
            if (kb == null || Showtime.BeforeTheGame)
                return;                                                   // (the opening is drawn over the frame: nothing of it can be picked yet)
            if (kb.f2Key.wasPressedThisFrame && !Writing)
            {
                if (!Picking)
                    StartPicking();
                else
                    Open(null, new Vector2(frame.Root.worldBound.width * 0.5f, frame.Root.worldBound.height * 0.5f));
            }
            else if (kb.escapeKey.wasPressedThisFrame && Picking)
                Leave();
        }

        // ------------------------------------------------------------------ the picker

        /// <summary>The clock is held and the pointer picks: the next click names the thing a note is about.</summary>
        public void StartPicking()
        {
            var frame = FrameRoot.Current;
            if (frame == null || frame.Root == null || Picking || Writing)
                return;
            Picking = true;
            GameSpeed.Hold(Why);
            UiFocus.Take(this, false);
            // THE PICKER'S PARTS ARE A FILE (KAPMAN_PLAN step 2d, 14 Oct 2026): copies of Game/UI/Parts/NotePicker.uxml
            var parts = FrameParts.Current;
            var picker = parts != null ? parts.notePicker : null;
            veil = FrameParts.Make<VisualElement>(picker, "note-veil");
            veil.pickingMode = PickingMode.Position;
            veil.RegisterCallback<PointerMoveEvent>(e => Ring(Under(e.position)));
            veil.RegisterCallback<PointerDownEvent>(e =>
            {
                e.StopPropagation();
                Open(Under(e.position), e.position);
            });
            mark = FrameParts.Make<VisualElement>(picker, "note-mark");
            mark.pickingMode = PickingMode.Ignore;
            mark.style.display = DisplayStyle.None;
            hint = FrameParts.Make<VisualElement>(picker, "note-hint");
            hint.pickingMode = PickingMode.Ignore;
            foreach (var child in hint.Children())
                child.pickingMode = PickingMode.Ignore;
            frame.Root.Add(veil);
            frame.Root.Add(mark);
            frame.Root.Add(hint);
        }

        /// <summary>What lies under a place on the screen, the picker's own veil set aside.</summary>
        VisualElement Under(Vector2 pos)
        {
            if (veil == null || veil.panel == null)
                return null;
            veil.pickingMode = PickingMode.Ignore;
            var hit = veil.panel.Pick(pos);
            veil.pickingMode = PickingMode.Position;
            return hit;
        }

        void Ring(VisualElement hit)
        {
            if (mark == null)
                return;
            var frame = FrameRoot.Current;
            if (hit == null || frame == null || hit == frame.Root)
            {
                mark.style.display = DisplayStyle.None;
                return;
            }
            Rect r = frame.Root.WorldToLocal(hit.worldBound);
            mark.style.display = DisplayStyle.Flex;
            mark.style.left = r.x - 3f;
            mark.style.top = r.y - 3f;
            mark.style.width = r.width + 6f;
            mark.style.height = r.height + 6f;
        }

        // ------------------------------------------------------------------ the slip

        /// <summary>A note about a thing on the screen (null: the whole screen), opened at once: the picture is taken
        /// first, with the thing ringed, then the slip comes up.</summary>
        public void Open(VisualElement hit, Vector2 where)
        {
            var frame = FrameRoot.Current;
            if (frame == null || frame.Root == null || Writing)
                return;
            if (!Picking)
                StartPicking();
            Picking = false;
            Writing = true;
            at = where;
            target = hit != null && hit != frame.Root ? hit : null;
            targetWords = WordsOf(target, where);
            if (targetWords != null && target != null && target.Contains(targetWords))
                target = targetWords;                                    // (words that take no clicks themselves: the note is about them)
            targetText = targetWords != null ? targetWords.text ?? "" : "";
            targetPath = PathOf(target);
            targetWindow = WindowOf(target);
            targetBox = target != null ? target.worldBound : frame.Root.worldBound;
            About = AboutWords(frame);
            Ring(target);
            if (hint != null)
                hint.style.display = DisplayStyle.None;
            if (veil != null)
                veil.pickingMode = PickingMode.Position;              // (the game under the slip takes no clicks)
            if (!counted)
            {
                Count = CountInFile();
                counted = true;
            }
            shot = "";
            try
            {
                Directory.CreateDirectory(Path.Combine(Folder, "shots"));
                shot = "shots/note_" + (Count + 1).ToString("0000", CultureInfo.InvariantCulture) + ".png";
                ScreenCapture.CaptureScreenshot(Path.Combine(Folder, shot));
            }
            catch (Exception e)
            {
                shot = "";
                Debug.LogWarning("Kapman notes: no picture of the screen (" + e.Message + ")");
            }
            StartCoroutine(SlipAfterThePicture());
        }

        IEnumerator SlipAfterThePicture()
        {
            // (the picture is taken at this frame's end, without the slip. Two frames, not WaitForEndOfFrame, which
            // never comes in a batch run)
            yield return null;
            yield return null;
            var frame = FrameRoot.Current;
            if (!Writing || frame == null || frame.Root == null)
                yield break;
            // THE SLIP IS A FILE (KAPMAN_PLAN step 2d, 14 Oct 2026): a copy of Game/UI/Parts/NoteSlip.uxml, filled in
            var parts = FrameParts.Current;
            slip = FrameParts.Make<VisualElement>(parts != null ? parts.noteSlip : null, "note-slip");
            FrameParts.In<Label>(slip, "note-about").text = "About " + About + ".";
            NoteBox = FrameParts.In<TextField>(slip, "note-box");
            NoteBox.multiline = true;
            WordsBox = null;
            if (targetWords != null && targetText.Trim().Length > 0)
            {
                WordsBox = FrameParts.In<TextField>(slip, "note-words");
                WordsBox.multiline = true;
                WordsBox.SetValueWithoutNotify(targetText);
            }
            else
            {
                slip.Q<VisualElement>("note-words-ask")?.RemoveFromHierarchy();   // (the thing has no words: not wanted this time)
                slip.Q<VisualElement>("note-words")?.RemoveFromHierarchy();
            }
            FrameParts.In<Button>(slip, "note-leave").clicked += Leave;
            FrameParts.In<Button>(slip, "note-save").clicked += Save;
            slip.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    e.StopPropagation();
                    Leave();
                }
                else if ((e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) && (e.ctrlKey || e.commandKey))
                {
                    e.StopPropagation();
                    Save();
                }
            }, TrickleDown.TrickleDown);
            frame.Root.Add(slip);
            NoteBox.Focus();
        }

        static Label Quiet(string text)
        {
            var l = FrameUi.P(text);
            l.AddToClassList("frame-note-quiet");
            return l;
        }

        static TextField Box(bool tall)
        {
            var f = new TextField { multiline = true };
            f.AddToClassList("frame-note-field");
            if (tall)
                f.AddToClassList("frame-note-tall");
            return f;
        }

        /// <summary>Writes the note down, puts new words on the thing for now, and leaves the mode.</summary>
        public void Save()
        {
            if (!Writing)
                return;
            string note = NoteBox != null ? (NoteBox.value ?? "").Trim() : "";
            string words = WordsBox != null ? (WordsBox.value ?? "").Trim() : "";
            bool newWords = WordsBox != null && words.Length > 0 && words != targetText.Trim();
            if (note.Length == 0 && !newWords)
            {
                Leave();                                                 // (nothing written: nothing kept)
                return;
            }
            int n = Count + 1;
            var game = KapmanGame.Current;
            string date = game != null ? FrameGame.DateOfDay(game.Day) : "";
            var port = FrameGame.PortHere;
            string place = port != null ? "at " + port.name : "at sea";
            var frame = FrameRoot.Current;
            Rect all = frame != null && frame.Root != null ? frame.Root.worldBound : new Rect(0f, 0f, Screen.width, Screen.height);
            string real = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            try
            {
                Directory.CreateDirectory(Folder);
                var md = new StringBuilder();
                if (!File.Exists(Path.Combine(Folder, "play_notes.md")))
                    md.Append("# Notes made while playing\n\nMade in the game with F2. Newest last. A note is done when its line in play_notes.jsonl says \"done\": true.\n");
                md.Append("\n## Note ").Append(n).Append(" (").Append(real).Append(")\n\n");
                md.Append("- In the game: ").Append(date.Length > 0 ? date + ", " : "").Append(place).Append('\n');
                md.Append("- About: ").Append(About).Append('\n');
                if (targetPath.Length > 0)
                    md.Append("- The thing: ").Append(targetPath).Append('\n');
                md.Append("- Where on the screen: ").Append(Num(at.x)).Append(", ").Append(Num(at.y)).Append(" of ").Append(Num(all.width)).Append(" x ").Append(Num(all.height)).Append('\n');
                if (shot.Length > 0)
                    md.Append("- Picture: ").Append(shot).Append('\n');
                if (note.Length > 0)
                    md.Append("\n**Change:** ").Append(note.Replace("\r", "")).Append('\n');
                if (newWords)
                    md.Append("\n**It says:** ").Append(targetText.Trim()).Append("\n\n**It should say:** ").Append(words.Replace("\r", "")).Append('\n');
                File.AppendAllText(Path.Combine(Folder, "play_notes.md"), md.ToString(), new UTF8Encoding(false));

                var js = new StringBuilder("{");
                Pair(js, "n", n.ToString(CultureInfo.InvariantCulture), false);
                Pair(js, "real", real, true);
                Pair(js, "game_date", date, true);
                Pair(js, "place", place, true);
                Pair(js, "about", About, true);
                Pair(js, "window", targetWindow, true);
                Pair(js, "thing", targetPath, true);
                Pair(js, "says", targetText, true);
                Pair(js, "should_say", newWords ? words : "", true);
                Pair(js, "note", note, true);
                Pair(js, "x", Num(at.x), false);
                Pair(js, "y", Num(at.y), false);
                Pair(js, "box", "[" + Num(targetBox.x) + "," + Num(targetBox.y) + "," + Num(targetBox.width) + "," + Num(targetBox.height) + "]", false);
                Pair(js, "screen", "[" + Num(all.width) + "," + Num(all.height) + "]", false);
                Pair(js, "shot", shot, true);
                Pair(js, "done", "false", false);
                js.Length -= 1;
                js.Append("}\n");
                File.AppendAllText(Path.Combine(Folder, "play_notes.jsonl"), js.ToString(), new UTF8Encoding(false));
                Count = n;
                FrameLog.Add(newWords ? $"Note {n} is written down, and the new words are shown for now." : $"Note {n} is written down.");
            }
            catch (Exception e)
            {
                Debug.LogError("Kapman notes: the note could not be written to " + Folder + " (" + e.Message + ")");
                FrameLog.Add("The note could not be written down. The reason is in the game's own log.");
            }
            if (newWords && targetWords != null && targetWords.panel != null)
                targetWords.text = words;
            Leave();
        }

        /// <summary>Out of the mode: the picker, the ring and the slip go, and the clock is let go.</summary>
        public void Leave()
        {
            if (Picking || Writing)
                quietUntil = Time.frameCount + 1;
            Picking = false;
            Writing = false;
            StopAllCoroutines();
            veil?.RemoveFromHierarchy();
            mark?.RemoveFromHierarchy();
            hint?.RemoveFromHierarchy();
            slip?.RemoveFromHierarchy();
            veil = mark = hint = slip = null;
            NoteBox = WordsBox = null;
            target = null;
            targetWords = null;
            GameSpeed.Release(Why);
            UiFocus.Release(this);
        }

        // ------------------------------------------------------------------ what a thing is, in words

        /// <summary>The words of a thing: its own if it is words, else those of the nearest thing above it that is (a
        /// click on a button's picture is a click on the button).</summary>
        static TextElement WordsOf(VisualElement v, Vector2 at)
        {
            if (v == null)
                return null;
            // words that let clicks through to what they stand on (a window's name on its bar): the smallest under the pointer
            TextElement under = null;
            float least = float.MaxValue;
            v.Query<TextElement>().ForEach(t =>
            {
                Rect r = t.worldBound;
                if (string.IsNullOrWhiteSpace(t.text) || !r.Contains(at) || r.width * r.height >= least)
                    return;
                least = r.width * r.height;
                under = t;
            });
            if (under != null)
                return under;
            int up = 0;
            for (var e = v; e != null && up < 4; e = e.parent, up++)
                if (e is TextElement t && !string.IsNullOrWhiteSpace(t.text))
                    return t;
            return null;
        }

        /// <summary>"frame-win (Voyage) > frame-ln > Label": the thing's place in the screen's tree, from the window down.</summary>
        static string PathOf(VisualElement v)
        {
            if (v == null)
                return "";
            var parts = new List<string>();
            for (var e = v; e != null && parts.Count < 7; e = e.parent)
            {
                string cls = "";
                foreach (string c in e.GetClasses())
                    if (!c.StartsWith("unity-", StringComparison.Ordinal))
                    {
                        cls = c;
                        break;
                    }
                string one = !string.IsNullOrEmpty(e.name) ? e.name + (cls.Length > 0 ? "." + cls : "") : cls.Length > 0 ? cls : e.GetType().Name;
                parts.Add(one);
                if (e.ClassListContains("frame-win") || e.ClassListContains("frame-float") || e.ClassListContains("frame-root"))
                    break;
            }
            parts.Reverse();
            return string.Join(" > ", parts);
        }

        /// <summary>The name of the window or the card a thing stands in ("" for none).</summary>
        static string WindowOf(VisualElement v)
        {
            for (var e = v; e != null; e = e.parent)
            {
                if (e.ClassListContains("frame-win"))
                    return (e.Q<Label>(null, "frame-win-name")?.text ?? e.Q<Label>(null, "frame-win-title")?.text ?? "").Trim();
                if (e.ClassListContains("frame-float"))
                    return (e.Q<Label>(null, "frame-float-title")?.text ?? "a card").Trim();
            }
            return "";
        }

        string AboutWords(FrameRoot frame)
        {
            if (target == null)
                return "the whole screen";
            string where = targetWindow.Length > 0 ? " in the window \"" + targetWindow + "\"" : "";
            string text = targetText.Trim().Replace("\n", " ");
            if (text.Length > 90)
                text = text.Substring(0, 90) + "...";
            if (text.Length > 0)
                return "the words \"" + text + "\"" + where;
            bool inFrame = false;
            for (var e = target; e != null; e = e.parent)
                inFrame |= e == frame.Root;
            if (!inFrame)
                return "the chart, at this place";
            return targetWindow.Length > 0 ? "a part of the window \"" + targetWindow + "\"" : "a part of the screen (" + targetPath + ")";
        }

        // ------------------------------------------------------------------ the files

        int CountInFile()
        {
            try
            {
                string path = Path.Combine(Folder, "play_notes.jsonl");
                if (!File.Exists(path))
                    return 0;
                int n = 0;
                foreach (string line in File.ReadAllLines(path))
                    if (line.Trim().Length > 0)
                        n++;
                return n;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        static string Num(float v) => Mathf.RoundToInt(v).ToString(CultureInfo.InvariantCulture);

        static void Pair(StringBuilder js, string key, string value, bool quoted)
        {
            js.Append('"').Append(key).Append("\":");
            if (!quoted)
                js.Append(value);
            else
            {
                js.Append('"');
                foreach (char c in value ?? "")
                    switch (c)
                    {
                        case '"': js.Append("\\\""); break;
                        case '\\': js.Append("\\\\"); break;
                        case '\n': js.Append("\\n"); break;
                        case '\r': break;
                        case '\t': js.Append("\\t"); break;
                        default:
                            if (c < 32)
                                js.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                            else
                                js.Append(c);
                            break;
                    }
                js.Append('"');
            }
            js.Append(',');
        }
    }
}
