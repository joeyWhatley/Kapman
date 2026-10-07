using System;
using System.Collections.Generic;
using System.Reflection;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// MAIN MENU - the title at Play and the pause menu on [Esc] (Plans/main_menu.md).
    ///   The title: over the ship at her berth, the camera circling her, the game clock held: New game (the game runs
    ///   from its start), Continue (the latest saved game), Load a saved game, Quit.
    ///   The pause menu: everything stops (a true pause) and the scene frosts: Resume, Save the game, Load a saved game,
    ///   New game, Quit; loading, a new game and quitting ask first.
    /// A new game mid-game clears the game's state as entering Play does, and reloads the scene (the title skipped once).
    /// Same look as the text panel: black see-through glass, a thin black line, square corners.
    /// </summary>
    [DefaultExecutionOrder(-99)]       // after the UI documents (-100); before the market and the map see [Esc] (they close on it)
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        public StyleSheet style;
        [Tooltip("Show the title when Play starts (off: straight into the game, for quick tests).")]
        public bool showTitle = true;
        [Tooltip("Degrees a second the camera circles the ship behind the title.")]
        public float titleOrbit = 3f;

        public static MainMenu Current { get; private set; }
        /// <summary>True while the title or the pause menu is up.</summary>
        public bool Open { get; private set; }
        public bool OnTitle { get; private set; }
        /// <summary>The menu's own root (the show fades it, and dresses the title).</summary>
        public VisualElement Root => root;

        static bool skipTitleOnce;

        VisualElement root, box, list;
        Label title, sub, note;
        string floorClass;                             // the floor under the text now on the root (TextFloor)
        readonly List<(Button button, Action act)> buttons = new List<(Button, Action)>();
        Action back;                                   // [Esc] in a sub-list: where it goes
        bool liveDate;                                 // the line under the title is the game's date (kept up to date)
        // ported 4 Oct: the rebuild's camera is OrbitCamera (no orbit speed of its own: the title turns its yaw each
        // frame), and the title holds the game through GameSpeed (the first build held the clock itself: GameClock.held)
        OrbitCamera view;
        const string TitleHold = "title";

        void Awake() => Current = this;

        void OnEnable()
        {
            var docRoot = GetComponent<UIDocument>().rootVisualElement;
            root?.RemoveFromHierarchy();
            if (style != null && !docRoot.styleSheets.Contains(style))
                docRoot.styleSheets.Add(style);
            // this document nests in the game's (its object is a child of '--- GAME ---'): fill the screen, and let the
            // clicks through while the menu's shut
            docRoot.style.position = Position.Absolute;
            docRoot.style.left = 0;
            docRoot.style.top = 0;
            docRoot.style.right = 0;
            docRoot.style.bottom = 0;
            docRoot.pickingMode = PickingMode.Ignore;
            root = new VisualElement();
            root.AddToClassList("menu-root");
            floorClass = null;
            box = new VisualElement();
            box.AddToClassList("menu-box");
            title = new Label();
            title.AddToClassList("menu-title");
            sub = new Label();
            sub.AddToClassList("menu-sub");
            list = new VisualElement();
            list.AddToClassList("menu-list");
            note = new Label();
            note.AddToClassList("menu-note");
            box.Add(title);
            box.Add(sub);
            box.Add(list);
            box.Add(note);
            root.Add(box);
            docRoot.Add(root);
            root.style.display = DisplayStyle.None;
        }

        void Start()
        {
            if (showTitle && !skipTitleOnce)
                ShowTitle();
            skipTitleOnce = false;
        }

        void OnDisable() => UiFocus.Release(this);

        // ---------------------------------------------------------------- the title

        public void ShowTitle()
        {
            OnTitle = true;
            OpenMenu(frost: false);
            GameSpeed.Hold(TitleHold);                                    // (the game clock held while the title is up)
            view = Camera.main != null ? Camera.main.GetComponent<OrbitCamera>() : null;
            TitleList();
        }

        void TitleList()
        {
            var game = KapmanGame.Current;
            Fill("Kāpman", game != null ? game.Date : "", "");
            liveDate = true;
            Add("New game", () => Showtime.NewGame(CloseTitle));
            var saves = SaveGame.Slots();
            if (saves.Count > 0)
                Add($"Continue: {Describe(saves[0])}", () => Showtime.Cut(() => LoadSlot(saves[0].slot, ask: false)));
            if (saves.Count > 0)
                Add("Load a saved game", () => SlotList(TitleList, ask: false));
            SoundLine(TitleList);
            Add("Quit", Quit);
            back = null;
        }

        /// <summary>A new game from the title, as its New game line does (the show's click anywhere).</summary>
        public void StartNewFromTitle()
        {
            if (OnTitle)
                Showtime.NewGame(CloseTitle);
        }

        void CloseTitle()
        {
            GameSpeed.Release(TitleHold);
            OnTitle = false;
            CloseMenu();
        }

        // ---------------------------------------------------------------- the pause menu

        public void Pause()
        {
            if (Open)
                return;
            GameSpeed.Hold("menu");
            OpenMenu(frost: true);
            PauseList();
        }

        void PauseList()
        {
            var game = KapmanGame.Current;
            Fill("Paused", game != null ? game.Date : "", "");
            liveDate = true;
            Add("Resume", Resume);
            Add("Save the game", () => note.text = SaveGame.Save(SaveGame.NewSlot()));
            if (SaveGame.Slots().Count > 0)
                Add("Load a saved game", () => SlotList(PauseList, ask: true));
            Add("New game", () => Ask("Start a new game?", NewGame, PauseList));
            if (ChartWorld.Current != null)
                Add("The clock", ClockList);
            SoundLine(PauseList);
            Add("Quit", () => Ask("Quit the game?", Quit, PauseList));
            back = Resume;
        }

        public void Resume()
        {
            GameSpeed.Release("menu");
            CloseMenu();
        }

        /// <summary>The clock slows by itself when something needs the player (ruling 26 of 7 Oct 2026): one line a reason,
        /// each a switch. A press turns it on or off, and the list is drawn again. Kept on this machine.</summary>
        void ClockList()
        {
            Fill("The clock slows by itself for", "A press turns one on or off.", "");
            foreach (ClockReason why in Enum.GetValues(typeof(ClockReason)))
            {
                var reason = why;
                Add((ClockSettings.On(reason) ? "On: " : "Off: ") + ClockSettings.Says(reason), () =>
                {
                    ClockSettings.Set(reason, !ClockSettings.On(reason));
                    ClockList();
                });
            }
            Add("Back", PauseList);
            back = PauseList;
        }

        /// <summary>The setting of the sound, where the scene has the chart's (ChartSound): a press takes the next of off,
        /// low, middling and loud, and the list is drawn again with it.</summary>
        void SoundLine(Action again)
        {
            if (ChartSound.Current == null)
                return;
            Add("Sound: " + ChartSound.SettingWord, () =>
            {
                ChartSound.NextSetting();
                again();
            });
        }

        // ---------------------------------------------------------------- saved games

        void SlotList(Action from, bool ask)
        {
            Fill("Load a saved game", "", "");
            foreach (var s in SaveGame.Slots())
            {
                var slot = s.slot;
                Add($"{s.slot}: {Describe(s)}", () =>
                {
                    if (ask)
                        Ask($"Load {slot}?", () => LoadSlot(slot, ask: false), () => SlotList(from, ask));
                    else
                        LoadSlot(slot, ask: false);
                });
            }
            Add("Back", from);
            back = from;
        }

        static string Describe(SaveGame.SlotInfo s)
        {
            string game = string.IsNullOrEmpty(s.date) ? "" : s.date + (string.IsNullOrEmpty(s.where) ? "" : ", " + s.where) + ". ";
            return $"{game}Saved {s.when:d MMM yyyy, HH:mm}.";
        }

        void LoadSlot(string slot, bool ask)
        {
            // the pause lifted and the title's hold given back first: the loaded game runs as it was saved
            GameSpeed.Release("menu");
            if (OnTitle)
                GameSpeed.Release(TitleHold);
            string fact = SaveGame.Load(slot);
            if (!fact.StartsWith("Loaded"))
            {
                GameSpeed.Hold(OnTitle ? TitleHold : "menu");
                note.text = fact;                                  // (why not: it stays open)
                return;
            }
            OnTitle = false;
            CloseMenu();
            Voyage.Say(fact);
        }

        // ---------------------------------------------------------------- a new game, quitting

        /// <summary>Clears the game's state as entering Play does, and starts the scene again.</summary>
        public void NewGame()
        {
            Time.timeScale = 1f;
            // the scene stops first (nothing runs on the cleared state before it's gone), then the state is cleared
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                go.SetActive(false);
            ClearAllState();
            skipTitleOnce = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>Every [RuntimeInitializeOnLoadMethod(SubsystemRegistration)] method in the Kapman assemblies: what
        /// entering Play runs to start each play clean (the project enters Play without a domain reload).</summary>
        public static void ClearAllState()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.GetName().Name.StartsWith("Kapman"))
                    continue;
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = Array.FindAll(e.Types, t => t != null);
                }
                foreach (var t in types)
                    foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    {
                        var a = m.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>();
                        if (a != null && a.loadType == RuntimeInitializeLoadType.SubsystemRegistration && m.GetParameters().Length == 0)
                            m.Invoke(null, null);
                    }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState() => skipTitleOnce = false;

        void Quit()
        {
            Autosave.Quitting();                                          // (while the game is still whole; asked again by the quit itself, it saves once)
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void Ask(string question, Action yes, Action no)
        {
            Fill(question, "What isn't saved is lost.", "");
            Add("Yes", yes);
            Add("No", no);
            back = no;
        }

        // ---------------------------------------------------------------- the panel

        void OpenMenu(bool frost)
        {
            Open = true;
            UiFocus.Take(this, frost);
            root.style.display = DisplayStyle.Flex;
            root.EnableInClassList("menu-title-screen", OnTitle);
        }

        void CloseMenu()
        {
            Open = false;
            UiFocus.Release(this);
            root.style.display = DisplayStyle.None;
            list.Clear();
            buttons.Clear();
        }

        void Fill(string titleText, string subText, string noteText)
        {
            liveDate = false;
            title.text = titleText;
            sub.text = subText;
            sub.style.display = string.IsNullOrEmpty(subText) ? DisplayStyle.None : DisplayStyle.Flex;
            note.text = noteText;
            list.Clear();
            buttons.Clear();
        }

        void Add(string label, Action act)
        {
            int n = buttons.Count;
            var b = new Button(() => act()) { text = $"{n + 1}. {label}" };
            b.AddToClassList("menu-option");
            list.Add(b);
            buttons.Add((b, act));
        }

        void Update()
        {
            TextFloor.Keep(root, GetComponent<UIDocument>().panelSettings, ref floorClass);   // (no text under 18 true pixels)
            var kb = Keyboard.current;
            if (kb == null || Showtime.Busy)
                return;
            if (Open && liveDate && KapmanGame.Current != null)
                sub.text = KapmanGame.Current.Date;
            if (OnTitle && view != null)
                view.yaw += titleOrbit * Time.unscaledDeltaTime;   // the camera circling the ship behind the title
            if (Open)
            {
                for (int i = 0; i < Mathf.Min(9, buttons.Count); i++)
                    if (kb[Key.Digit1 + i].wasPressedThisFrame)
                    {
                        buttons[i].act();
                        return;
                    }
                if (kb.escapeKey.wasPressedThisFrame)
                    back?.Invoke();
                return;
            }
            // [Esc] pauses when nothing else is using it (the map and the market close on it; it skips an event shot)
            if (kb.escapeKey.wasPressedThisFrame
                && !Frame.PlayNotes.Busy
                && !(MapView.Current != null && MapView.Current.IsOpen)
                && !(MarketPanel.Current != null && MarketPanel.Current.IsOpen)
                && !(Sim.ShotDirector.Current != null && Sim.ShotDirector.Current.Now != Sim.ShotDirector.Shot.None))
                Pause();
        }
    }
}
