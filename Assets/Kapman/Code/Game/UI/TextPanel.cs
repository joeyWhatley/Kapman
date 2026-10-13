using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// TEXT PANEL - the game's text interactions: a place or a happening, its options, and what came of the choice.
    /// No story is written here (the user writes all of it): every slot has a key, and the text for it comes from
    /// the writing table (Data/writing.json: key -> text). Until a key has text the panel shows a marked placeholder,
    /// so what each step means - its INTENTION and its OUTCOME - is always plain:
    ///   title       the place or the happening ("Dorestad")
    ///   [text]      the user's writing for the key, or "[write: key]"
    ///   intention   what this step is ("You come in to the jetties")
    ///   options     each a key and an intention ("Go to the market")
    ///   outcome     what happened, as facts ("Bought 2 casks of wine for 16 deniers")
    /// The graphs drive it (Text: Show, Text: Add option, Text: Wait for choice, Text: Show outcome, Text: Hide).
    /// Laid out in Game/UI/TextPanel.uxml, an option in Game/UI/Parts/TextOption.uxml (14 Oct 2026).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class TextPanel : MonoBehaviour
    {
        /// <summary>The name of the panel's root element in Game/UI/TextPanel.uxml.</summary>
        public const string RootName = "text-panel";

        public StyleSheet style;
        [Tooltip("The user's writing: a JSON table of key -> text (Data/writing.json).")]
        public TextAsset writing;

        /// <summary>The one on screen (graphs use it through the Text nodes).</summary>
        public static TextPanel Current { get; private set; }

        /// <summary>The key the port's own menu is shown with (the ports' graph: "Ports - in port").</summary>
        public const string PortMenuKey = "port.arrive";

        /// <summary>The option chosen since the last Show (-1: none yet).</summary>
        public int Choice { get; private set; } = -1;
        public bool Open { get; private set; }
        /// <summary>The key of what is shown (the place or the happening), "" when shut.</summary>
        public string Key { get; private set; } = "";

        // ---- shown elsewhere (7 Oct 2026, the frame): on the chart the frame draws what this panel holds (the port's
        // menu as lines of its Port window, a talk as a floating window), and the graphs go on driving this panel as ever
        /// <summary>Something else draws the panel and takes its keys: its own box stays out of sight.</summary>
        public static bool External;

        // ---- the graphs' waits (7 Oct 2026)
        /// <summary>A wait "for a choice, or so many seconds" counts the game's own seconds, not the wall's: nothing while
        /// the clock is held (a talk that holds the clock waits for its answer and does not answer itself meanwhile),
        /// and faster at 2x and 4x. The chart's frame sets it: sixty seconds is then an hour of the game's clock.</summary>
        public static bool WaitsByGameClock;
        /// <summary>Counts up when every wait for a choice is given up (<see cref="AbandonWaits"/>).</summary>
        public static int Abandoned { get; private set; }
        /// <summary>How many of the graphs' flows are waiting for a choice on the panel now. More than one is a fault:
        /// both would take the same choice.</summary>
        public static int Waiters;

        /// <summary>Every flow of the graphs that waits for a choice on the panel ends where it stands, and the panel
        /// shuts. For a moment the game is put somewhere else under them (a saved game loaded, a new game laid at
        /// another port): without it the old port's menu went on waiting behind the new one, and a choice was taken
        /// twice (the men paid twice, an errand done twice).</summary>
        public static void AbandonWaits()
        {
            Abandoned++;
            Waiters = 0;
            Current?.Hide();
        }

        /// <summary>A talk at sea is up (a ship met, raiders, the king's men, aground): anything on the panel but a
        /// port's own business (its menu, its talk, an errand).</summary>
        public static bool TalkAtSea =>
            Current != null && Current.Open && !Current.Key.StartsWith("port.", StringComparison.Ordinal);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            Waiters = 0;
            WaitsByGameClock = false;
        }

        /// <summary>The user's writing for a key, if there is any (no placeholder).</summary>
        public bool TryWriting(string key, out string text)
        {
            text = "";
            if (string.IsNullOrEmpty(key) || !texts.TryGetValue(key, out var t) || string.IsNullOrWhiteSpace(t))
                return false;
            text = t;
            return true;
        }

        /// <summary>What is shown, as the graphs set it: the title, the user's writing (or ""), the plain line under it,
        /// and what came of the choice ("" until there is one).</summary>
        public string TitleText { get; private set; } = "";
        public string StoryText { get; private set; } = "";
        public string IntentionText { get; private set; } = "";
        public string OutcomeText { get; private set; } = "";
        /// <summary>The options as shown, in order: the key, the words on the button (without its number), and what it means.</summary>
        public IReadOnlyList<(string key, string label, string tip)> Options => shownOptions;
        /// <summary>Counts up whenever what is shown changes (whoever draws it elsewhere looks again).</summary>
        public int Version { get; private set; }
        readonly List<(string key, string label, string tip)> shownOptions = new List<(string, string, string)>();

        /// <summary>Chooses option `index` (from 0), as a click on its button does.</summary>
        public void Pick(int index)
        {
            if (Open && index >= 0 && index < shownOptions.Count)
                Choose(index);
        }

        /// <summary>Chooses the first option with this key; false if none is offered now.</summary>
        public bool PickKey(string key)
        {
            if (!Open || Choice >= 0)
                return false;
            for (int i = 0; i < shownOptions.Count; i++)
                if (shownOptions[i].key == key)
                {
                    Choose(i);
                    return true;
                }
            return false;
        }

        VisualElement root, box, options;
        Label title, story, intention, outcome;
        readonly List<Button> buttons = new List<Button>();
        readonly Dictionary<string, string> texts = new Dictionary<string, string>();

        [Serializable]
        class Entry { public string key, text; }

        [Serializable]
        class WritingFile { public Entry[] entries = new Entry[0]; }

        void Awake()
        {
            Current = this;
            Waiters = 0;                                               // (a new scene: none of its flows waits yet)
            LoadWriting();
        }

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            var docRoot = doc.rootVisualElement;
            if (style != null && !docRoot.styleSheets.Contains(style))
                docRoot.styleSheets.Add(style);
            // THE PANEL IS LAID OUT IN A FILE (KAPMAN_PLAN step 2d, 14 Oct 2026): Game/UI/TextPanel.uxml, this document's
            // own Source Asset. Its parts are found by NAME and given their words; a part gone from the file is said so
            // in the Console and a plain stand-in is made. The documents of child objects (the chart, the market) nest in
            // this root and draw over it; only our own root is ours.
            root = docRoot.Q<VisualElement>(RootName);
            if (root == null || root.userData != null)
            {
                root?.RemoveFromHierarchy();                             // (our own of an earlier enabling)
                root = doc.visualTreeAsset != null ? doc.visualTreeAsset.Instantiate().Q<VisualElement>(RootName) : null;
                if (root == null)
                {
                    Debug.LogError("Kapman text panel: the UI Document of '" + name + "' has no Source Asset with an element named \"" + RootName +
                                   "\" (it should be Assets/Kapman/Game/UI/TextPanel.uxml). A bare panel is made.");
                    root = new VisualElement { name = RootName, pickingMode = PickingMode.Ignore };
                    root.AddToClassList("text-root");
                }
                root.RemoveFromHierarchy();
                docRoot.Add(root);
            }
            root.userData = this;                                         // (filled: it is not filled twice)
            box = Kapman.Game.Frame.FrameParts.In<VisualElement>(root, "box");
            title = Kapman.Game.Frame.FrameParts.In<Label>(root, "title");
            story = Kapman.Game.Frame.FrameParts.In<Label>(root, "story");
            intention = Kapman.Game.Frame.FrameParts.In<Label>(root, "intention");
            options = Kapman.Game.Frame.FrameParts.In<VisualElement>(root, "options");
            outcome = Kapman.Game.Frame.FrameParts.In<Label>(root, "outcome");
            Hide();
        }

        void LoadWriting()
        {
            texts.Clear();
            string json = writing != null ? writing.text : null;
            if (json == null)
            {
                // ported 4 Oct: nothing assigned in the scene: the writing table is read from the project's data folder
                // (Assets/Kapman/Game/Data/writing.json) so the panel works before the scene is wired; in a build the
                // asset must be assigned
                string path = System.IO.Path.Combine(Application.dataPath, "Kapman/Game/Data/writing.json");
                if (System.IO.File.Exists(path))
                {
                    Debug.LogWarning("Kapman text panel: no writing asset assigned; reading " + path + " (assign Game/Data/writing.json on the TextPanel).");
                    json = System.IO.File.ReadAllText(path);
                }
                else
                {
                    Debug.LogWarning("Kapman text panel: no writing asset assigned and none at " + path + ": every slot shows its [write: key] placeholder.");
                    return;
                }
            }
            var file = JsonUtility.FromJson<WritingFile>(json);
            foreach (var e in file?.entries ?? new Entry[0])
                if (!string.IsNullOrEmpty(e.key))
                    texts[e.key] = e.text ?? "";
        }

        /// <summary>The user's text for a key, or a marked placeholder.</summary>
        public string Writing(string key) =>
            !string.IsNullOrEmpty(key) && texts.TryGetValue(key, out var t) && !string.IsNullOrWhiteSpace(t) ? t : $"[write: {key}]";

        /// <summary>Opens the panel on a place or a happening (its options and outcome cleared). `frost`: blur the scene
        /// behind it (off for a meeting at sea, where the ships are what it's about).</summary>
        public void Show(string key, string titleText, string intentionText, bool frost = true)
        {
            Open = true;
            Key = key ?? "";
            UiFocus.Take(this, frost);                               // the number keys are the options' now
            Choice = -1;
            root.style.display = External ? DisplayStyle.None : DisplayStyle.Flex;
            title.text = titleText ?? "";
            // your writing for the key, if there is any (no placeholder on screen: the key is in the tooltip)
            bool written = texts.TryGetValue(key ?? "", out var w) && !string.IsNullOrWhiteSpace(w);
            TitleText = titleText ?? "";
            StoryText = written ? w : "";
            IntentionText = intentionText ?? "";
            OutcomeText = "";
            Version++;
            story.text = written ? w : "";
            story.style.display = written ? DisplayStyle.Flex : DisplayStyle.None;
            story.tooltip = $"[{key}]";
            intention.text = intentionText ?? "";
            intention.style.display = string.IsNullOrEmpty(intentionText) ? DisplayStyle.None : DisplayStyle.Flex;
            outcome.text = "";
            outcome.style.display = DisplayStyle.None;
            ClearOptions();
        }

        /// <summary>Changes the line under the title while the panel is open (a live status: a chase's numbers).</summary>
        public void SetIntention(string text)
        {
            if (intention == null)
                return;
            if (IntentionText != (text ?? ""))
            {
                IntentionText = text ?? "";
                Version++;
            }
            intention.text = text ?? "";
            intention.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        /// <summary>Adds an option (its key, for the writing, and its intention); returns its number from 0.</summary>
        public int AddOption(string key, string intentionText) => AddOption(key, intentionText, null);

        /// <summary>Adds an option with a label in the words of the place (Data/wording_793.json), its intention in
        /// small print under it; your writing for the key, if any, replaces the label.</summary>
        public int AddOption(string key, string intentionText, string label)
        {
            int index = buttons.Count;
            string written = texts.TryGetValue(key ?? "", out var t) && !string.IsNullOrWhiteSpace(t) ? t : null;
            // (a copy of Parts/TextOption.uxml)
            var parts = Kapman.Game.Frame.FrameParts.Current;
            var b = Kapman.Game.Frame.FrameParts.Make<Button>(parts != null ? parts.textOption : null, "option");
            b.clicked += () => Choose(index);
            b.tooltip = $"[{key}] {intentionText}";
            // just the label (the user's menu, 2026-10-01); what it means is in the tooltip
            string words = written ?? (string.IsNullOrEmpty(label) ? intentionText : label);
            b.text = $"{index + 1}. {words}";
            options.Add(b);
            buttons.Add(b);
            shownOptions.Add((key ?? "", words ?? "", intentionText ?? ""));
            Version++;
            return index;
        }

        public void ClearOptions()
        {
            options.Clear();
            buttons.Clear();
            shownOptions.Clear();
            Choice = -1;
            Version++;
        }

        /// <summary>Shows what came of the choice (facts; the user's text for the key above it if there is any).</summary>
        public void ShowOutcome(string key, string outcomeText)
        {
            outcome.style.display = DisplayStyle.Flex;
            string written = texts.TryGetValue(key ?? "", out var t) && !string.IsNullOrWhiteSpace(t) ? t + "\n" : "";
            outcome.text = written + "Outcome: " + outcomeText;
            OutcomeText = written + (outcomeText ?? "");
            Version++;
        }

        public void Hide()
        {
            Open = false;
            Key = "";
            Version++;
            UiFocus.Release(this);
            if (root != null)
                root.style.display = DisplayStyle.None;
        }

        void OnDisable() => UiFocus.Release(this);

        void Choose(int index)
        {
            if (Choice < 0)
                Choice = index;
        }

        void Update()
        {
            // the title first, then the port (the user, 2026-10-03: the title and Witla's choices showed at once): while
            // the title is up an open panel stays out of sight, and comes the moment the title closes
            bool onTitle = MainMenu.Current != null && MainMenu.Current.OnTitle;
            if (Open)
                root.style.display = onTitle || External ? DisplayStyle.None : DisplayStyle.Flex;
            // the number keys choose too
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (!Open || kb == null || (MainMenu.Current != null && MainMenu.Current.Open))
                return;                                              // (the main menu over it has the keys)
            if (External || EncounterPanel.Showing)
                return;                                              // (the frame, or a card over the port's menu, has them)
            for (int i = 0; i < Mathf.Min(9, buttons.Count); i++)
                if (kb[UnityEngine.InputSystem.Key.Digit1 + i].wasPressedThisFrame)
                    Choose(i);
        }
    }
}
