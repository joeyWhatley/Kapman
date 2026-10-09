using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Kapman.Game.Frame;
using Kapman.Game.ShipView;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// THE BEGINNING OF THE GAME, AS A GAME BEGINS (the user, 7 Oct 2026: "make the beginning flow and have some
    /// semblance of an actual studio game"; and, in their own words, what follows). One layer over everything else on
    /// the screen (it lives in the menu's document, which is the top one):
    ///
    ///   THE SLOW START. "A slow start like a loading psx, maybe a rotating ship at the bottom right as a fake loader."
    ///   Black; the purple comes up slowly; a small boat turns at the bottom right over the word Loading; nothing is
    ///   being loaded. Then the game's name, big and bold, and the title's menu under it. Any key goes straight there.
    ///
    ///   THE BACKDROP. "The PSX style filter from those screenshots but just the colour purple": a small picture of
    ///   slow purple bands, in five tones of Byzantium with the hard dots of a console between them, drawn a few
    ///   times a second and stretched over the screen.
    ///
    ///   NEW GAME. The name "fades away, then the text box slides up from the bottom slowly to the middle of the
    ///   screen and it expands, not slow but not fast, into the text rectangle". Somebody asks, a letter at a time:
    ///   "..." / "And from where do you hail?" (your people) / "And your name?" (typed, or given from the names of
    ///   that people) / "And by what do you swear?" (your faith: he needs your oath) / "And what were you, before
    ///   this?" (your past). Then it is shown who asks: the lender, "a cold aloof uninterested man, just cares about
    ///   getting what he's owed". He says the terms, and praises a boat that is not worth it.
    ///
    ///   THE SHOWROOM. "A rotating floating 3D model of the ship rotating like the old psx demos, no water, just the
    ///   pink void." A key or a click goes on.
    ///
    ///   THE FIRST MORNING. A cut through black, the place's name and the day across the screen, and the game.
    ///   "You're seemingly free to click around, but as you click something you're interrupted with multiple !!!
    ///   appearing. Then another text conversation with Ravn": he says how the game is worked, what must be done
    ///   before she sails (hands hired, the sail mended: FirstJobs), and what to buy and where to take it.
    ///
    /// Also: a cut through black for Continue, and a place's card each time she comes in somewhere.
    ///
    /// EVERY WORD SAID HERE IS A PLACEHOLDER for the user's own (the lender's name and byname: Trade.LenderName,
    /// Trade.LenderByname). Off in a batch run, where the tests play the game and read the screen, unless a test asks
    /// for it (<see cref="ForceOn"/>): there the old creation window asks the three choices, and none of this is seen.
    /// </summary>
    [DefaultExecutionOrder(-90)]                                      // (after the main menu, -99: it stands in the scene with it now)
    public sealed class Showtime : MonoBehaviour
    {
        public static Showtime Current { get; private set; }

        /// <summary>A test asks for the show in a batch run.</summary>
        public static bool ForceOn;
        public static bool On => ForceOn || !Application.isBatchMode;

        /// <summary>Something of the show is on, or the first click of the game is waited for: the menu and the frame
        /// leave the keys alone.</summary>
        public static bool Busy => Current != null && (Current.busy > 0 || Current.armed || Current.titleOnly);
        /// <summary>No card of the game opens yet: the show is on, or Ravn has not had his say.</summary>
        public static bool HoldsCards => Current != null && (Current.busy > 0 || Current.armed || Current.ravnUp);
        /// <summary>The title, the opening and the showroom: before the first morning (the music's own question).</summary>
        public static bool BeforeTheGame => Current != null && On && Current.before;
        /// <summary>The slow start has been played (or passed over) in this run of the game.</summary>
        public static bool StartPlayed { get; private set; }

        /// <summary>Seconds the fake loading lasts, and the box takes to come up from the bottom and to open.</summary>
        public static float LoadingSeconds = 6.5f, SlideSeconds = 2.8f, OpenSeconds = 0.5f;
        /// <summary>Seconds the marks of the first click shake before they blink out.</summary>
        public static float MarksShake = 1.1f;

        // ---- the words (placeholders, all of them)
        public const string GameName = "Kāpman";
        /// <summary>Who Ravn is, under his name while he speaks (a placeholder for the user's own words; he has no
        /// figures yet: the crew are a count until they are people).</summary>
        // (only what he is: the critic, 8 Oct 2026, of "Talks a great deal and means well" under his name: it "tells the player what to
        // think before he speaks")
        public const string RavnIs = "A Northman";
        public const string AskFirst = "...";
        // THE LENDER, WRITTEN AGAIN (the user, 8 Oct 2026: "I think wursing needs a re-script"; by the picker: "Cold, and eating", the
        // user's first brief for him, "a cold aloof uninterested man, just cares about getting what he's owed"). He does not look up
        // from his food, makes no joke, and calls a wreck a good boat. His questions are plain (the script critic: "From where do you
        // hail?" and "And by what do you swear?" were the user's own of 7 Oct, and are changed on that report, which the user took up).
        public const string AskPeople = "Where are you from?";
        /// <summary>
        /// The answers to that are homelands, not peoples, each as its own people wrote or said it (the user, 7 Oct 2026:
        /// "make the answers Homeland titles, not Frisian, make it Frisia, BUT in the same spelling originally used by
        /// that culture, not the modern versions of it"). PLACEHOLDERS FROM MEMORY, each to be checked against a source:
        /// Old Frisian Fresland with a long e; the Franks' own Latin Francia (in their speech, Frankono lant); Old
        /// English Engla land; Old Norse Danmork with the hooked o, as the Jelling stone's tanmaurk is normalised.
        /// A people not listed here is shown by its own name.
        /// </summary>
        public static readonly Dictionary<string, string> Homelands = new Dictionary<string, string>
        {
            ["frisian"] = "Fr\u0113sland", ["frankish"] = "Francia", ["english"] = "Engla land", ["danish"] = "Danm\u01EBrk",
            ["norse"] = "Nor\u00F0vegr", ["svea"] = "Sv\u00ED\u00FEj\u00F3\u00F0", ["irish"] = "\u00C9riu",
        };
        public const string AskName = "Your name?";
        /// <summary>What he says while the player picks a face (a placeholder of mine, in his voice).</summary>
        public const string AskFace = "Step into the light.";
        public const string AskFaith = "What do you swear by?";
        public const string AskPast = "And before this? What were you?";
        // WURSING'S VOICE (the user, 7 Oct 2026, late: "with Wursing, can you add mannerisms, and text quirks, don't make
        // him sound so AI please, more personality in the read"). He counts while he talks and does not look up: he
        // breaks off to finish a dozen, says a thing once and then to the stick, never uses two words where a number
        // will do, and is put out only when he loses the count. Still placeholders for the user's own words.
        // (his byname is said once and said what it means, in his own mouth: the user, twice, "I don't know what that other word means";
        // by the picker, "Keep it, said once". The last word of the second line is the meaning: keep the two together.)
        public static readonly string[] LenderLines =
        {
            "{name}. One notch. There. Sit if you like. I am eating.",
            "I am {lender}. On the quay they call me {byname}. The usurer. I do not lend. I sell boats.",
            "Yours is at the end of the bank. Six oars, a mast, a sail. A good boat. She has years in her.",
            "For her, {pounds} pounds of silver. {pence} pennies. I count them. I do not weigh them.",
            "You bring it at {day}. The day after, I bring two men and a rope, and I do not stay to hear why.",
            "There are four casks of wine in her. Mine. They are on the stick with the boat.",
            "That is all of it. Shut the door behind you.",
        };
        // (the plate under the boat says what is seen. The lender's joke that she floats was told here a second time and by Ravn a
        // third: the critic. It is Ravn's alone now.)
        public static readonly string[] PlateLines = { "Your boat.", "Six oars, a mast, and a sail of sorts." };
        public const string RavnWho = "He came with the boat. Nobody told you.";

        /// <summary>What is on now, for whoever watches: "", "loading", "name", "title", "slide", "asking", "lender",
        /// "showroom", "first morning", "free", "marks", "cut", "place".</summary>
        public string Stage { get; private set; } = "";
        /// <summary>The picks made so far in the opening's talk.</summary>
        public string PickedPeople { get; private set; } = "";
        public string PickedFaith { get; private set; } = "";
        public string PickedPast { get; private set; } = "";
        public string PickedName { get; private set; } = "";
        /// <summary>The art slot of the face picked ("" none: no faces on this machine, or not yet).</summary>
        public string PickedFace { get; private set; } = "";
        public VisualElement Box => box;
        public Label Words => words;
        public VisualElement Choices => list;
        public TextField NameField { get; private set; }
        public VisualElement BigName => nameRow;
        /// <summary>The title's sea is up (the stage's storm, seen from above).</summary>
        public Image SeaPicture => seaPicture;
        /// <summary>The title's menu is up (after a click where there are saved games, or Esc); else the title is the
        /// name alone and a click anywhere plays.</summary>
        public bool MenuShown => menuShown;
        public VisualElement Loader => loader;
        public Image ShipPicture => shipFull;
        /// <summary>The first click of the game is waited for (then the marks, then Ravn).</summary>
        public bool Armed => armed;

        VisualElement under, over, loader, box, row, col, list, card, veil, marks, plate, speaker, scene, sceneSpeaker, sceneBox, flash;
        BalloonTail tail;
        Image backdrop, loaderShip, shipFull;
        Label loaderWord, who, words, small, go, cardBig, cardSmall, sceneWords, sceneGo, sceneWho;
        FrameFace sceneFace;
        VisualElement nameRow, titleVeil;
        Image seaPicture;
        readonly List<VisualElement> letters = new List<VisualElement>();
        readonly List<float> letterAway = new List<float>();
        readonly List<float> letterShape = new List<float>();              // a letter's picture: its width to its height (0: plain text)
        float nameIn;
        bool titleOnly, menuShown, titleClicked, leaving;
        FrameFace face;
        MainMenu menu;
        Texture2D purple;
        Color32[] purplePixels;
        float purpleNext;
        int busy;
        bool before = true, skip, starting, clicked, armed, ravnUp, flagOwed, wasFrameSeen, portKnown;
        string lastPort = "";
        Vector2 clickAt;
        readonly System.Random dice = new System.Random();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = null;
            StartPlayed = false;
            ForceOn = false;
            FrameRoot.AskedElsewhere = false;
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
                Destroy(gameObject);                                      // (never two)
                return;
            }
            Current = this;
            // (with the show on, the three choices are asked by the lender, never by the creation window)
            FrameRoot.AskedElsewhere = On;
        }

        static void TryStart()
        {
            if (Current != null || MainMenu.Current == null)
                return;
            new GameObject("Showtime").AddComponent<Showtime>();          // (its Awake takes the place)
        }

        void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
                FrameRoot.AskedElsewhere = false;
                GameSpeed.Release(Hold_);
                GameSpeed.Release(SceneHold);
                UiFocus.Release(this);
            }
            if (purple != null)
                Destroy(purple);
        }

        const string Hold_ = "opening";

        // ------------------------------------------------------------------ the layer

        bool Build()
        {
            menu = MainMenu.Current;
            if (menu == null)
                return false;
            var doc = menu.GetComponent<UIDocument>();
            var docRoot = doc != null ? doc.rootVisualElement : null;
            if (docRoot == null)
                return false;
            if (over != null && over.parent == docRoot && under.parent == docRoot)
                return true;
            under?.RemoveFromHierarchy();
            over?.RemoveFromHierarchy();

            // THE SHOW IS LAID OUT IN A FILE (KAPMAN, step 2): Game/UI/Showtime.uxml, named in Resources/FrameParts. A copy
            // of its two layers is taken here, one put under the menu and one over it; each part is found by its name
            // and shown, hidden and filled as the show goes. Open the file in UI Builder to move or restyle a part.
            var parts = FrameParts.Current;
            VisualElement made = parts != null && parts.showtime != null ? parts.showtime.Instantiate() : new VisualElement { name = "Showtime.uxml (not set in Resources/FrameParts)" };
            under = FrameParts.In<VisualElement>(made, "show-under");
            over = FrameParts.In<VisualElement>(made, "show-over");
            under.RemoveFromHierarchy();
            over.RemoveFromHierarchy();
            under.pickingMode = PickingMode.Ignore;
            over.pickingMode = PickingMode.Ignore;

            backdrop = FrameParts.In<Image>(under, "backdrop");
            backdrop.scaleMode = ScaleMode.StretchToFill;
            nameRow = FrameParts.In<VisualElement>(under, "name-row");
            nameRow.Clear();
            letters.Clear();
            letterAway.Clear();
            letterShape.Clear();
            int nth = 0;
            foreach (char c in GameName)
            {
                // (the user, 7 Oct 2026: "the title font needs to have texture to it": each letter is a picture of the art
                // folder, title/title.letter.N.png, the user's own when it is drawn, else the worn painted one made for a
                // placeholder; with no picture at all it is the plain letter of the frame's face)
                nth++;
                var picture = FrameArt.Load("title/title.letter." + nth + ".png");
                VisualElement l;
                if (picture != null && picture.height > 0)
                {
                    var drawn = new Image { image = picture, scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
                    drawn.AddToClassList("show-letter-picture");
                    l = drawn;
                    letterShape.Add(picture.width / (float)picture.height);
                }
                else
                {
                    l = Text("show-letter", c.ToString());
                    letterShape.Add(0f);
                }
                letters.Add(l);
                letterAway.Add(0f);
                nameRow.Add(l);
            }
            seaPicture = FrameParts.In<Image>(under, "sea-picture");
            seaPicture.scaleMode = ScaleMode.ScaleAndCrop;
            titleVeil = FrameParts.In<VisualElement>(under, "title-click");
            titleVeil.RegisterCallback<PointerDownEvent>(e =>
            {
                e.StopPropagation();
                titleClicked = true;
            });
            loader = FrameParts.In<VisualElement>(under, "loader");
            loaderShip = FrameParts.In<Image>(under, "loader-ship");
            loaderShip.scaleMode = ScaleMode.ScaleAndCrop;
            loaderWord = FrameParts.In<Label>(under, "loader-word");

            shipFull = FrameParts.In<Image>(over, "ship-full");
            shipFull.scaleMode = ScaleMode.ScaleAndCrop;
            plate = FrameParts.In<VisualElement>(over, "plate");
            box = FrameParts.In<VisualElement>(over, "opening-box");
            box.RegisterCallback<PointerDownEvent>(e =>
            {
                // (a click on the words goes on; a click on a button, or in the name's box, is that thing's own)
                for (var t = e.target as VisualElement; t != null && t != box; t = t.parent)
                    if (t is Button || t is TextField)
                        return;
                clicked = true;
            });
            row = FrameParts.In<VisualElement>(over, "box-row");
            col = FrameParts.In<VisualElement>(over, "box-col");
            words = FrameParts.In<Label>(over, "words");
            list = FrameParts.In<VisualElement>(over, "list");
            small = FrameParts.In<Label>(over, "quiet");
            go = FrameParts.In<Label>(over, "go");
            // (a speech balloon: the one who speaks stands outside it, under its tail, and is not seen until he says
            // who he is: the user, 7 Oct 2026, "it needs to come across like a text bubble")
            tail = FrameParts.In<BalloonTail>(over, "tail");
            speaker = FrameParts.In<VisualElement>(over, "speaker");
            face = FrameParts.In<FrameFace>(over, "face");
            who = FrameParts.In<Label>(over, "who");
            // (the black of a cut: above the box and the showroom, under a place's card)
            black = FrameParts.In<VisualElement>(over, "black");
            card = FrameParts.In<VisualElement>(over, "card");
            cardBig = FrameParts.In<Label>(over, "card-big");
            cardSmall = FrameParts.In<Label>(over, "card-small");
            // (a scene told over the black, one line at a time: Scene)
            scene = FrameParts.In<VisualElement>(over, "scene");
            sceneWords = FrameParts.In<Label>(over, "scene-words");
            sceneGo = FrameParts.In<Label>(over, "scene-go");
            sceneSpeaker = FrameParts.In<VisualElement>(over, "scene-speaker");
            sceneBox = FrameParts.In<VisualElement>(over, "scene-box");
            sceneFace = FrameParts.In<FrameFace>(over, "scene-face");
            sceneWho = FrameParts.In<Label>(over, "scene-who");
            // (the red of a hit: over everything of a scene)
            flash = FrameParts.In<VisualElement>(over, "flash");
            veil = FrameParts.In<VisualElement>(over, "first-click");
            veil.RegisterCallback<PointerDownEvent>(e =>
            {
                e.StopPropagation();
                if (armed && busy == 0)
                    FirstClick(e.position);
                else
                    clicked = true;
            });
            marks = FrameParts.In<VisualElement>(over, "marks");
            docRoot.Insert(0, under);
            docRoot.Add(over);

            foreach (var v in new VisualElement[] { backdrop, seaPicture, nameRow, titleVeil, loader, shipFull, plate, box, tail, speaker, card, veil, scene, flash })
                Hide(v);
            Black(0f);
            if (purple == null)
            {
                purple = new Texture2D(PurpleW, PurpleH, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
                purplePixels = new Color32[PurpleW * PurpleH];
            }
            backdrop.image = purple;
            return true;
        }

        VisualElement black;

        static VisualElement Plain(string cls)
        {
            var v = new VisualElement { pickingMode = PickingMode.Ignore };
            v.AddToClassList(cls);
            return v;
        }

        static Label Text(string cls, string text)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            return l;
        }

        static void Hide(VisualElement v) => v.style.display = DisplayStyle.None;

        static void Show(VisualElement v, float opacity = 1f)
        {
            v.style.display = DisplayStyle.Flex;
            v.style.opacity = opacity;
        }

        void Black(float opacity)
        {
            black.style.opacity = opacity;
            black.style.display = opacity > 0.004f ? DisplayStyle.Flex : DisplayStyle.None;
            // (closed or closing, it takes the clicks: nothing under it is pressed unseen)
            black.pickingMode = opacity > 0.02f ? PickingMode.Position : PickingMode.Ignore;
        }

        float W => over.resolvedStyle.width;
        float H => over.resolvedStyle.height;

        /// <summary>The text's sizes go with the screen's height, never under the frame's smallest.</summary>
        void Sizes()
        {
            float h = H;
            if (float.IsNaN(h) || h < 100f)
                return;
            float body = Mathf.Max(22f, h * 0.030f), quiet = Mathf.Max(18f, h * 0.021f);
            for (int i = 0; i < letters.Count; i++)
            {
                if (letterShape[i] > 0f)
                {
                    // (a letter's picture is its whole line, head to foot: a third of the screen's height)
                    letters[i].style.height = h * 0.34f;
                    letters[i].style.width = h * 0.34f * letterShape[i];
                }
                else
                    letters[i].style.fontSize = h * 0.19f;
            }
            float shipWide = h * 0.2f;
            loaderShip.style.width = shipWide;
            loaderShip.style.height = shipWide;
            loaderWord.style.fontSize = quiet;
            who.style.fontSize = body;
            words.style.fontSize = body;
            small.style.fontSize = quiet;
            go.style.fontSize = quiet;
            cardBig.style.fontSize = Mathf.Max(30f, h * 0.07f);
            cardSmall.style.fontSize = quiet;
            sceneWords.style.fontSize = body;
            sceneWho.style.fontSize = body;
            sceneGo.style.fontSize = quiet;
            list.style.fontSize = Mathf.Max(20f, h * 0.024f);
            plate.style.fontSize = body;
        }

        // ------------------------------------------------------------------ the purple

        const int PurpleW = 256, PurpleH = 144;
        static readonly Color32[] PurpleTones =
        {
            new Color32(0x1d, 0x0a, 0x1a, 255), new Color32(0x2e, 0x12, 0x33, 255), new Color32(0x4a, 0x1b, 0x47, 255),
            new Color32(0x70, 0x29, 0x63, 255), new Color32(0x86, 0x40, 0x7b, 255),
        };
        static readonly float[] Bayer = { 0f, 8f, 2f, 10f, 12f, 4f, 14f, 6f, 3f, 11f, 1f, 9f, 15f, 7f, 13f, 5f };

        /// <summary>The backdrop drawn again: slow bands of purple, in five tones with a console's dots between.</summary>
        void Purple(float t)
        {
            int n = PurpleTones.Length;
            for (int y = 0; y < PurpleH; y++)
            {
                float fy = y / (float)PurpleH;
                float bandY = Mathf.Sin(fy * 9f - t * 0.33f);
                for (int x = 0; x < PurpleW; x++)
                {
                    float fx = x / (float)PurpleW;
                    float v = 0.30f + 0.26f * fy
                              + 0.13f * Mathf.Sin(fx * 7f + t * 0.41f + 1.7f * Mathf.Sin(fy * 5f + t * 0.23f))
                              + 0.09f * bandY;
                    float d = (Bayer[(y & 3) * 4 + (x & 3)] + 0.5f) / 16f - 0.5f;
                    int tone = Mathf.Clamp(Mathf.FloorToInt(v * n + d), 0, n - 1);
                    purplePixels[y * PurpleW + x] = PurpleTones[tone];
                }
            }
            purple.SetPixels32(purplePixels);
            purple.Apply(false);
        }

        // ------------------------------------------------------------------ each frame

        void Update()
        {
            if (!Build())
                return;
            var parent = over.parent;
            if (parent.IndexOf(over) != parent.childCount - 1)
                over.BringToFront();
            if (parent.IndexOf(under) != 0)
                under.SendToBack();
            Sizes();
            FrameRoot.AskedElsewhere = On;
            if (!On)
            {
                before = false;
                return;
            }
            // (Ravn's death has a scene of its own, begun when the card of the rope men is shut: Voyage/Feud.cs)
            if (busy == 0 && !armed && !before)
                Feud.Tick();
            // the backdrop is drawn only while it is seen, a dozen times a second (a console's pace)
            if (backdrop.resolvedStyle.display != DisplayStyle.None && Time.unscaledTime >= purpleNext)
            {
                purpleNext = Time.unscaledTime + 1f / 12f;
                Purple(Time.unscaledTime);
            }
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (starting && ((kb != null && kb.anyKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame)))
                skip = true;
            if (loader.resolvedStyle.display != DisplayStyle.None)
            {
                var hook = ShipStageHook.Current;
                loaderShip.image = hook != null ? hook.StagePicture : null;
                int dots = (int)(Time.unscaledTime * 2.2f) % 4;
                loaderWord.text = "Loading" + new string('.', dots);
            }
            if (shipFull.resolvedStyle.display != DisplayStyle.None)
            {
                var hook = ShipStageHook.Current;
                shipFull.image = hook != null ? hook.StagePicture : null;
            }
            if (seaPicture.resolvedStyle.display != DisplayStyle.None)
            {
                var hook = ShipStageHook.Current;
                seaPicture.image = hook != null && hook.TitleSea ? hook.StagePicture : null;
            }
            if (nameRow.resolvedStyle.display != DisplayStyle.None)
                Letters();
            Balloon();
            titleOnly = false;

            // the slow start, once, when the title comes up; the title's dress while it is up
            if (menu.OnTitle)
            {
                before = true;
                if (!StartPlayed && !starting)
                    StartCoroutine(SlowStart());
                else if (!starting && busy == 0)
                    TitleNow(kb);
            }
            else if (!StartPlayed && Time.timeSinceLevelLoad > 1f && busy == 0)
            {
                StartPlayed = true;                                      // (straight into a game: nothing to open)
                before = false;
            }
            else if (busy == 0 && !armed && !Self.Chosen && ChartWorld.Current != null && !ChartWorld.SaveLoaded
                     && FrameRoot.Current != null && KapmanGame.Current != null && !menu.Open)
                BeginOpening(null);                                      // (a new game begun from the pause menu: no title, the same talk)
            else if (busy == 0 && before && StartPlayed && Self.Chosen)
                before = false;                                          // (a game loaded from the title, the title skipped)

            // the lender's flag, once the cards' state is there to carry it
            if (flagOwed && ContentFlags.Ready)
            {
                ContentFlags.Set(Trade.LenderFlag);
                flagOwed = false;
            }

            // the first key of the game does what the first click does
            if (armed && busy == 0 && kb != null && kb.anyKey.wasPressedThisFrame)
                FirstClick(new Vector2(W * 0.5f, H * 0.45f));

            // she has come in somewhere: the place's card, over the game
            string here = FrameGame.PortHere != null ? FrameGame.PortHere.id : "";
            if (!portKnown || menu.OnTitle || before || !Self.Chosen || busy > 0)
            {
                portKnown = true;
                lastPort = here;
            }
            else if (here != lastPort)
            {
                lastPort = here;
                if (here.Length > 0 && !menu.Open)
                    StartCoroutine(PlaceCard(FrameGame.PortHere.name, DayWords(), false));
            }
        }

        /// <summary>
        /// THE TITLE (the user, 7 Oct 2026: "a top down of a stormy ocean with rain, with the title of the game animated
        /// in the middle, and it's click anywhere to play but it doesn't say that, just the title"). The sea behind, the
        /// name riding it, and nothing else. A click anywhere, or a key, plays: the letters go under one by one; then,
        /// where there are saved games, the menu comes up (Continue stands first); where there are none, the new game's
        /// talk begins. Esc brings the menu up, or puts it away: the sound's setting and Quit are there.
        /// </summary>
        void TitleNow(Keyboard kb)
        {
            var root = menu.Root;
            if (root != null)
            {
                root.EnableInClassList("show-title", true);
                var date = root.Q<Label>(null, "menu-sub");
                if (date != null)
                    date.style.display = DisplayStyle.None;              // (the menu sets it itself each time it fills: put away again)
                root.style.display = menuShown ? DisplayStyle.Flex : DisplayStyle.None;
            }
            if (!Sea(true) && backdrop.resolvedStyle.display == DisplayStyle.None)
                Show(backdrop);                                          // (no 3D view in this run: the purple stands in for the sea)
            if (!menuShown && nameRow.resolvedStyle.display == DisplayStyle.None)
            {
                for (int i = 0; i < letterAway.Count; i++)
                    letterAway[i] = 0f;
                Show(nameRow);
            }
            nameIn = Mathf.MoveTowards(nameIn, 1f, Time.unscaledDeltaTime / 1.6f);
            titleOnly = !menuShown;
            if (menuShown)
                Hide(titleVeil);
            else
                Show(titleVeil, 0f);
            bool esc = kb != null && kb.escapeKey.wasPressedThisFrame;
            bool key = kb != null && kb.anyKey.wasPressedThisFrame && !esc;
            if (esc)
            {
                menuShown = !menuShown;
                if (root != null)
                    root.style.opacity = StyleKeyword.Null;
            }
            else if (!menuShown && (titleClicked || key))
                StartCoroutine(LeaveTitle());
            titleClicked = false;
        }

        /// <summary>The balloon's tail hangs from its lower edge, a little in from the left, and the speaker stands under
        /// the tail's point. Both go wherever the balloon is this frame, and are put away with it.</summary>
        void Balloon()
        {
            if (box.resolvedStyle.display == DisplayStyle.None)
            {
                if (tail.resolvedStyle.display != DisplayStyle.None)
                    Hide(tail);
                if (speaker.resolvedStyle.display != DisplayStyle.None)
                    Hide(speaker);
                return;
            }
            Rect b = box.worldBound;
            if (float.IsNaN(b.width) || b.width < 8f)
                return;
            float tw = Mathf.Max(30f, H * 0.04f), th = Mathf.Max(26f, H * 0.045f);
            tail.style.left = b.x + b.width * 0.1f;
            tail.style.top = b.yMax - 3f;                                // (over the balloon's own lower line, which it opens)
            tail.style.width = tw;
            tail.style.height = th;
            speaker.style.left = b.x + b.width * 0.1f - 30f;
            speaker.style.top = b.yMax + th + 6f;
        }

        /// <summary>The stage's storm behind the title, on or off. False when there is no stage to show it.</summary>
        bool Sea(bool on)
        {
            var hook = ShipStageHook.Current;
            if (hook == null)
            {
                Hide(seaPicture);
                return false;
            }
            if (hook.TitleSea != on)
            {
                hook.ShowroomLines = 0;
                hook.TitleSea = on;
            }
            if (on && seaPicture.resolvedStyle.display == DisplayStyle.None)
                Show(seaPicture);
            else if (!on)
                Hide(seaPicture);
            return true;
        }

        /// <summary>The name's letters as they stand this frame: each rises in after the one before it, rides the sea
        /// with a slow bob and roll of its own, and (LettersGo) heels over and goes under.</summary>
        void Letters()
        {
            float t = Time.unscaledTime, h = H;
            int n = letters.Count;
            var hookNow = ShipStageHook.Current;
            var afloat = hookNow != null && hookNow.Stage != null && hookNow.Stage.TitleSea && hookNow.Stage.TitleLettersUp ? hookNow.Stage : null;
            for (int i = 0; i < n; i++)
            {
                float k = Mathf.Clamp01(nameIn * (n + 2f) - i);
                if (afloat != null)
                {
                    // (the letters are things on the title's sea: the stage floats them; here they are only timed)
                    if (i < afloat.TitleIn.Length)
                        afloat.TitleIn[i] = k;
                    if (i < afloat.TitleAway.Length)
                        afloat.TitleAway[i] = letterAway[i];
                    letters[i].style.opacity = 0f;
                    continue;
                }
                k = k * k * (3f - 2f * k);
                float away = letterAway[i];
                float bob = Mathf.Sin(t * 1.25f + i * 0.9f) * h * 0.013f * (1f - away);
                float rise = (1f - k) * h * 0.07f;
                float sink = away * away * h * 0.2f;
                letters[i].style.translate = new Translate(0f, bob + rise + sink);
                letters[i].style.rotate = new Rotate(new Angle(Mathf.Sin(t * 0.9f + i * 1.7f) * 3.5f * (1f - away) + away * (i % 2 == 0 ? 30f : -26f), AngleUnit.Degree));
                letters[i].style.scale = new Scale(Vector3.one * (1f - 0.4f * away));
                letters[i].style.opacity = k * (1f - away);
            }
        }

        /// <summary>The letters go under, one after another in no order, and the name is put away.</summary>
        IEnumerator LettersGo()
        {
            int n = letters.Count;
            var order = new List<int>();
            for (int i = 0; i < n; i++)
                order.Insert(dice.Next(order.Count + 1), i);
            const float each = 0.6f, gap = 0.11f;
            float total = each + gap * (n - 1), t = 0f;
            while (t < total)
            {
                t += Time.unscaledDeltaTime;
                for (int j = 0; j < n; j++)
                    letterAway[order[j]] = Mathf.Clamp01((t - gap * j) / each);
                yield return null;
            }
            for (int i = 0; i < n; i++)
                letterAway[i] = 1f;
            Hide(nameRow);
        }

        IEnumerator LeaveTitle()
        {
            busy++;
            leaving = true;
            Stage = "letters";
            Hide(titleVeil);
            yield return LettersGo();
            yield return Wait(0.3f);
            leaving = false;
            if (SaveGame.Slots().Count > 0)
            {
                // (there is a game to go on with: the menu, under the sea's sky)
                menuShown = true;
                var root = menu.Root;
                if (root != null)
                {
                    root.style.display = DisplayStyle.Flex;
                    yield return Fade(a => root.style.opacity = a, 0f, 1f, 0.5f);
                    root.style.opacity = StyleKeyword.Null;
                }
                Stage = "menu";
                busy--;
            }
            else
            {
                busy--;
                menu.StartNewFromTitle();
            }
        }

        static string DayWords()
        {
            var game = KapmanGame.Current;
            if (game == null)
                return "";
            var clock = LevelClock.Current;
            string date = FrameGame.DateOfDay(game.Day);
            return clock != null ? date + ", " + FrameUi.PartOfDay(clock.HourOfDay) : date;
        }

        // ------------------------------------------------------------------ the pieces of time

        IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds && !(starting && skip))
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        IEnumerator Fade(Action<float> set, float from, float to, float seconds)
        {
            float t = 0f;
            while (t < seconds && !(starting && skip))
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / seconds);
                set(Mathf.Lerp(from, to, k * k * (3f - 2f * k)));
                yield return null;
            }
            set(to);
        }

        /// <summary>Enter, Space or a click on the box: go on.</summary>
        bool Go()
        {
            var kb = Keyboard.current;
            bool key = kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame);
            bool was = clicked;
            clicked = false;
            return key || was;
        }

        // ------------------------------------------------------------------ the slow start

        IEnumerator SlowStart()
        {
            starting = true;
            busy++;
            skip = false;
            before = true;
            Stage = "loading";
            var root = menu.Root;
            if (root != null)
            {
                root.EnableInClassList("show-title", true);
                root.style.display = DisplayStyle.None;
            }
            menuShown = false;
            Show(backdrop);
            Hide(nameRow);
            Hide(seaPicture);
            Black(1f);
            yield return Wait(0.9f);

            // the fake loading: the boat turns at the bottom right while the purple comes up
            var hook = ShipStageHook.Current;
            if (hook != null)
            {
                ShipStage.ShowroomVoid = new Color(0.44f, 0.16f, 0.39f);   // (Byzantium: the purple's own middle)
                hook.ShowroomLines = 300;
                hook.Showroom = true;
            }
            Show(loader, 0f);
            yield return Fade(a => loader.style.opacity = a, 0f, 1f, 0.5f);
            yield return Fade(Black, 1f, 0f, Mathf.Min(3.2f, LoadingSeconds * 0.5f));
            yield return Wait(Mathf.Max(0f, LoadingSeconds - 3.2f));
            yield return Fade(a => loader.style.opacity = a, 1f, 0f, 0.6f);
            Hide(loader);
            yield return Fade(Black, 0f, 1f, 0.5f);
            if (hook != null)
                hook.Showroom = false;

            // a cut to the sea: a storm from above, in the purple; then the name rises out of it
            Stage = "name";
            nameIn = 0f;
            bool sea = Sea(true);
            if (sea)
                Hide(backdrop);
            yield return Wait(0.7f);
            yield return Fade(Black, 1f, 0f, 1.6f);
            yield return Wait(0.5f);
            for (int i = 0; i < letterAway.Count; i++)
                letterAway[i] = 0f;
            Show(nameRow);
            yield return Fade(k => nameIn = k, 0f, 1f, 2.2f);

            // (passed over, or run out: the title as it stands)
            starting = false;
            skip = false;
            Hide(loader);
            if (hook != null)
                hook.Showroom = false;
            Black(0f);
            if (!Sea(true))
                Show(backdrop);
            else
                Hide(backdrop);
            for (int i = 0; i < letterAway.Count; i++)
                letterAway[i] = 0f;
            nameIn = 1f;
            Show(nameRow);
            titleClicked = false;
            Stage = "title";
            StartPlayed = true;
            busy--;
        }

        // ------------------------------------------------------------------ a cut through black

        /// <summary>Something that changes the whole screen, done behind the black: it closes, the thing is done, it
        /// lifts on the game. With the show off, the thing is just done.</summary>
        public static void Cut(Action middle)
        {
            if (Current == null || !On || Current.over == null)
            {
                middle?.Invoke();
                return;
            }
            if (Current.busy > 0)
                return;                                                  // (one cut at a time)
            Current.StartCoroutine(Current.Through(middle));
        }

        IEnumerator Through(Action middle)
        {
            busy++;
            Stage = "cut";
            yield return Fade(Black, 0f, 1f, 0.45f);
            middle?.Invoke();
            yield return Wait(0.5f);
            if (!menu.OnTitle)
            {
                // (a game is on: the title's dress goes; if the thing was refused, the menu is still there and stays dressed)
                Hide(backdrop);
                Hide(nameRow);
                Hide(titleVeil);
                Sea(false);
                menuShown = false;
                menu.Root?.EnableInClassList("show-title", false);
                before = false;
            }
            yield return Fade(Black, 1f, 0f, 0.7f);
            Stage = "";
            busy--;
        }

        // ------------------------------------------------------------------ a scene told over the black

        /// <summary>
        /// A SCENE TOLD OVER THE BLACK (the user, 9 Oct 2026: "Ravn needs an impactful death scene"; and of its first
        /// cut, words alone in the middle of the dark: "doesn't flow well enough ... just doesn't hit", "have it more
        /// Pokémon paced with dialogue"; and of the black it first stood on: "Instead of black use that Byzantium
        /// themed purple you made in the intro that was cool and a great theme to keep going"). The game stops: the
        /// clock is held, the keys are the show's, the music dies away. THE PURPLE OF THE OPENING comes up over the
        /// whole screen (the title's own slow bands). At its foot stands ONE BOX OF TALK, as Pokémon has it: over it
        /// the face and the name of whoever speaks (nobody, for the teller), in it a sentence or two at a time, said
        /// a letter at a time, and Enter or a click brings the next. A line may BREAK OFF (ToldLine.cut): it is said
        /// to its last letter and gone at once, nobody is waited for, and the purple stands empty a moment. Then a
        /// card with a name and what stands under it, and the purple lifts on the game. Nothing is chosen in it.
        /// `done` is called when it is over. With the show off (a batch run), or while another part of the show is
        /// on, nothing is shown and `done` is called at once.
        /// </summary>
        public static void Scene(IReadOnlyList<ToldLine> lines, string big, string small, Action done)
        {
            if (Current == null || !On || Current.over == null || Current.busy > 0)
            {
                done?.Invoke();
                return;
            }
            Current.StartCoroutine(Current.Told(lines, big, small, done));
        }

        /// <summary>A scene is being told now (the music dies away for it).</summary>
        public static bool Telling => Current != null && Current.Stage == "scene";
        /// <summary>Which line of the scene is up (1 the first; 0: none), which box of that line (1 the first), and is the
        /// box said in full and waiting for the key.</summary>
        public int SceneLine { get; private set; }
        public int SceneBox { get; private set; }
        public bool SceneWaits { get; private set; }
        /// <summary>The words of the box that is up, whole ("" for none), and who says them ("" for the teller); and
        /// what stands on the name's card while it is up.</summary>
        public string SceneText { get; private set; } = "";
        public string SceneWho { get; private set; } = "";
        public string CardText => card != null && card.resolvedStyle.display != DisplayStyle.None ? cardBig.text + " / " + cardSmall.text : "";
        /// <summary>Seconds the black takes to close for a scene and the dark before its first line; and, for a line
        /// that breaks off, how long its last letters stand and how long the dark stands after.</summary>
        public static float SceneClose = 1.4f, SceneDark = 0.9f, SceneCutHold = 0.3f, SceneCutDark = 1.5f;

        // ---- A HIT (the user, 9 Oct 2026: "a layer of transparent bloody red filter flash and shake the screen when
        // it says he's killed in the retro Nintendo style"). Hard steps, nothing eased.
        /// <summary>Seconds one step of a hit lasts (the red and the shake change together, a step at a time).</summary>
        public static float SceneHitStep = 0.05f;
        /// <summary>How red the filter is at each step: on, off, on, and then down in two steps. TWO FLASHES AND NO
        /// MORE, inside half a second: three or more flashes in a second is what harms some eyes.</summary>
        static readonly float[] HitRed = { 0.62f, 0.62f, 0f, 0.62f, 0.62f, 0.62f, 0.34f, 0.34f, 0.16f, 0.16f };
        /// <summary>Where the picture stands at each step, in hundredths of the screen's height: thrown hard to one
        /// side and the other, a little less each time.</summary>
        static readonly Vector2[] HitShake =
        {
            new Vector2(1.6f, 0.5f), new Vector2(-1.6f, -0.5f), new Vector2(1.3f, -0.4f), new Vector2(-1.3f, 0.4f), new Vector2(1.0f, 0.3f), new Vector2(-1.0f, -0.3f),
            new Vector2(0.7f, -0.2f), new Vector2(-0.7f, 0.2f), new Vector2(0.4f, 0.1f), new Vector2(-0.4f, -0.1f), new Vector2(0.2f, 0f), new Vector2(-0.2f, 0f),
        };
        /// <summary>How many hits this run of the game has shown, whether one is on now, and how red the filter is now.</summary>
        public int SceneHits { get; private set; }
        public bool SceneStruck { get; private set; }
        public float SceneRed { get; private set; }

        /// <summary>A hit: the red filter and the shaken picture, step by step, and then all as it was.</summary>
        IEnumerator Struck()
        {
            SceneHits++;
            SceneStruck = true;
            int steps = Mathf.Max(HitRed.Length, HitShake.Length), step = -1;
            float t = 0f;
            Show(flash, 0f);
            // (the purple is drawn a little over the screen's edges while it is thrown about, so no edge of the game shows)
            backdrop.style.scale = new Scale(new Vector2(1.08f, 1.08f));
            while (true)
            {
                int now = (int)(t / Mathf.Max(0.01f, SceneHitStep));
                if (now >= steps)
                    break;
                if (now != step)
                {
                    step = now;
                    SceneRed = step < HitRed.Length ? HitRed[step] : 0f;
                    flash.style.opacity = SceneRed;
                    var by = (step < HitShake.Length ? HitShake[step] : Vector2.zero) * (H * 0.01f);
                    var thrown = new Translate(Mathf.Round(by.x), Mathf.Round(by.y));
                    scene.style.translate = thrown;
                    backdrop.style.translate = thrown;
                }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            SceneRed = 0f;
            Hide(flash);
            scene.style.translate = new Translate(0f, 0f);
            backdrop.style.translate = new Translate(0f, 0f);
            backdrop.style.scale = new Scale(Vector2.one);
            SceneStruck = false;
        }

        const string SceneHold = "scene";

        IEnumerator Told(IReadOnlyList<ToldLine> lines, string big, string small, Action done)
        {
            busy++;
            Stage = "scene";
            UiFocus.Take(this, false);
            GameSpeed.Hold(SceneHold);
            Show(veil, 0f);                                              // (a click anywhere goes on, and none falls through to the game)
            // (the purple of the opening comes up over the game: its bands are drawn while it is seen, Update)
            Show(backdrop, 0f);
            yield return Fade(a => backdrop.style.opacity = a, 0f, 1f, SceneClose);
            yield return Wait(SceneDark);
            sceneWords.text = "";
            sceneGo.style.visibility = Visibility.Hidden;
            for (int i = 0; lines != null && i < lines.Count; i++)
            {
                var line = lines[i];
                if (line == null || string.IsNullOrEmpty(line.text))
                    continue;
                // who speaks: the face and the name over the box; the teller has neither, and no balloon round the words
                bool teller = string.IsNullOrEmpty(line.who);
                sceneBox.EnableInClassList("teller", teller);
                sceneSpeaker.style.visibility = teller ? Visibility.Hidden : Visibility.Visible;
                if (!teller)
                {
                    sceneFace.Who = line.face.Length > 0 ? line.face : line.who;
                    if (line.slot.Length > 0)
                        sceneFace.Slot = line.slot;
                    sceneWho.text = line.who;
                }
                var boxes = TextReveal.Pages(line.text, TextReveal.PageChars);
                for (int b = 0; b < boxes.Count; b++)
                {
                    bool breaksOff = line.cut && b == boxes.Count - 1;
                    SceneLine = i + 1;
                    SceneBox = b + 1;
                    SceneWaits = false;
                    SceneText = boxes[b];
                    SceneWho = line.who ?? "";
                    sceneWords.text = boxes[b];
                    Show(scene);
                    // (a hit: red, and the picture shaken, as this line comes up)
                    if (line.hit && b == 0)
                        StartCoroutine(Struck());
                    var reveal = new TextReveal();
                    reveal.Add(sceneWords);
                    reveal.StartAt(0);
                    // (a hit's own words stand whole at once, under the red: they are read as the screen is struck)
                    if (line.hit && b == 0)
                        reveal.Finish();
                    clicked = false;
                    yield return null;
                    // (and nothing is gone on from while the screen is still being struck)
                    while (line.hit && b == 0 && SceneStruck)
                    {
                        clicked = false;
                        yield return null;
                    }
                    while (!reveal.Done)
                    {
                        // (a line that breaks off cannot be hurried to its end: it ends when it ends)
                        if (!breaksOff && Go())
                            reveal.Finish();
                        else
                            reveal.Tick(Time.unscaledDeltaTime);
                        yield return null;
                    }
                    if (breaksOff)
                    {
                        yield return Wait(SceneCutHold);
                        // (what cut it off is a hit: it comes at once, with no empty moment between)
                        bool struckNext = i + 1 < lines.Count && lines[i + 1] != null && lines[i + 1].hit;
                        if (!struckNext)
                        {
                            Hide(scene);
                            SceneText = "";
                            yield return Wait(SceneCutDark);
                        }
                        clicked = false;
                        continue;
                    }
                    sceneGo.style.visibility = Visibility.Visible;
                    SceneWaits = true;
                    yield return null;
                    while (!Go())
                        yield return null;
                    SceneWaits = false;
                    sceneGo.style.visibility = Visibility.Hidden;
                }
            }
            while (SceneStruck)
                yield return null;
            SceneLine = SceneBox = 0;
            SceneText = SceneWho = "";
            Hide(scene);
            yield return Wait(0.6f);
            // (the name and what stands under it, on the purple; the purple lifts on the game while they stand)
            if (!string.IsNullOrEmpty(big))
            {
                cardBig.text = big;
                cardSmall.text = small ?? "";
                Show(card, 0f);
                yield return Fade(a => card.style.opacity = a, 0f, 1f, 0.6f);
                yield return Wait(2.2f);
                yield return Fade(a => backdrop.style.opacity = a, 1f, 0f, 1.4f);
                yield return Wait(0.7f);
                yield return Fade(a => card.style.opacity = a, 1f, 0f, 0.8f);
                Hide(card);
            }
            else
                yield return Fade(a => backdrop.style.opacity = a, 1f, 0f, 1.4f);
            Hide(backdrop);
            backdrop.style.opacity = 1f;
            Hide(veil);
            GameSpeed.Release(SceneHold);
            UiFocus.Release(this);
            Stage = "";
            busy--;
            done?.Invoke();
        }

        // ------------------------------------------------------------------ a new game: the talk with the lender

        /// <summary>The title's New game. With the show off the title just closes (the creation window asks).</summary>
        public static void NewGame(Action closeTitle)
        {
            if (Current == null || !On || Current.over == null || SelfBook.Current == null)
            {
                closeTitle?.Invoke();
                return;
            }
            if (Current.busy > 0)
                return;
            Current.BeginOpening(closeTitle);
        }

        // ------------------------------------------------------------------ THE OPENING IS A FLOW
        // (KAPMAN, step 3; the user, 7 Oct 2026: "everywhere you can make it visual scripting".) What follows the title
        // was one long sequence here. It is cut into its parts, each a thing that happens: the box comes up; who are
        // you; the game begins as answered; the lender speaks; the showroom; the first morning; the first click; the
        // marks; Ravn speaks. A GRAPH runs them in that order: "Opening - the lender, the boat, the first morning", on
        // this object (its nodes are in Code/Graphs/OpeningNodes.cs). Open the graph to read the opening left to
        // right, to put a wait or a sound between two parts, or to leave one out. In a scene with no such graph the
        // same parts are run in the same order from here (OpeningInCode).
        // Three things reach across the parts: the box takes the game in hand (the clock held, the keys the show's) and
        // the first morning gives it back; the first click is waited for and the marks answer it.

        /// <summary>Set by the graphs' side (Kapman.Graphs): starts the opening's graph and returns true; false where no
        /// graph of the scene listens for it.</summary>
        public static Func<bool> OpeningFlow;
        /// <summary>The opening that last began was run by its graph (false: by the same parts from code).</summary>
        public static bool OpeningByGraph { get; private set; }

        Action closeTitleNow;
        bool firstClicked;
        Vector2 firstClickAt;

        void BeginOpening(Action closeTitle)
        {
            closeTitleNow = closeTitle;
            firstClicked = false;
            OpeningByGraph = OpeningFlow != null && OpeningFlow();
            if (!OpeningByGraph)
                StartCoroutine(OpeningInCode());
        }

        IEnumerator OpeningInCode()
        {
            yield return OpeningBox();
            yield return OpeningQuestions();
            OpeningBegin();
            yield return OpeningLender();
            yield return OpeningShowroom();
            yield return OpeningFirstMorning();
            yield return OpeningFirstClick();
            yield return OpeningMarks();
            RavnSpeaks();
        }

        /// <summary>The purple, and the box: up from the bottom to the middle, then it opens. The game is taken in hand
        /// here (the clock held, the keys the show's) until the first morning.</summary>
        public IEnumerator OpeningBox()
        {
            Action closeTitle = closeTitleNow;
            closeTitleNow = null;
            busy++;
            before = true;
            Stage = "slide";
            UiFocus.Take(this, false);
            GameSpeed.Hold(Hold_);
            Show(backdrop);                                              // (under the sea, which fades into it)
            Hide(titleVeil);
            var root = menu.Root;
            if (closeTitle != null)
            {
                if (root != null && root.resolvedStyle.display != DisplayStyle.None)
                    yield return Fade(a => root.style.opacity = a, 1f, 0f, 0.4f);
                closeTitle();                                            // (the frame is up now, under the sea and the purple)
                if (root != null)
                {
                    root.style.opacity = StyleKeyword.Null;
                    root.EnableInClassList("show-title", false);
                }
                menuShown = false;
                if (nameRow.resolvedStyle.display != DisplayStyle.None)
                    yield return LettersGo();                            // (the letters go under, if they have not yet)
                if (seaPicture.resolvedStyle.display != DisplayStyle.None)
                {
                    yield return Fade(a => seaPicture.style.opacity = a, 1f, 0f, 1.5f);   // (the sea fades into the purple)
                    seaPicture.style.opacity = StyleKeyword.Null;
                }
            }
            Sea(false);
            Hide(nameRow);
            yield return Wait(0.6f);

            // the box: up from the bottom, slowly, to the middle; then it opens, not slow and not fast
            float w = W, h = H;
            float seedW = Mathf.Max(60f, w * 0.05f), seedH = Mathf.Max(36f, h * 0.04f), fullW = w * 0.58f, fullH = h * 0.30f;
            Hide(row);
            Hide(tail);
            Hide(speaker);
            list.Clear();
            Hide(small);
            Hide(go);
            words.text = "";
            box.style.width = seedW;
            box.style.height = seedH;
            box.style.minHeight = StyleKeyword.Null;
            box.style.top = Length.Percent(108f);
            Show(box);
            yield return Fade(p => box.style.top = Length.Percent(p), 108f, 50f, SlideSeconds);
            yield return Wait(0.25f);
            yield return Fade(k =>
            {
                box.style.width = Mathf.Lerp(seedW, fullW, k);
                box.style.height = Mathf.Lerp(seedH, fullH, k);
            }, 0f, 1f, OpenSeconds);
            box.style.height = StyleKeyword.Auto;
            box.style.minHeight = fullH;
            Show(row);
            Show(tail);
            yield return Wait(0.3f);
        }

        /// <summary>Somebody asks: the face, the land, the name, the faith, the past. What is answered is kept
        /// (PickedFace, PickedPeople, PickedName, PickedFaith, PickedPast) for the game to begin with.</summary>
        public IEnumerator OpeningQuestions()
        {
            // somebody asks
            Stage = "asking";
            var book = SelfBook.Current;
            yield return Say(AskFirst, true);

            // THE PLAYER'S OWN FACE, CHOSEN FIRST (the user, 7 Oct 2026, late: "you gotta replace the AI image of the player
            // and make it a choice at the very start out of all the choices"): every face of the pool (the user's own in
            // Art/final/portrait_pool, the stand-ins in Art/standin/portrait_pool), in rows; a click or its number takes
            // one. With no pool on a machine the step is not there, and the drawn portrait of the people stands.
            PickedFace = "";
            var faces = FacePool();
            if (faces.Count > 0)
                yield return AskTheFace(faces);

            var peoples = book.Offered(id => GameData.Port(id) != null);
            var options = new List<(string name, string small)>();
            foreach (var p in peoples)
            {
                var port = GameData.Port(p.home);
                string tongue = p.firstTongue.Length > 0 ? Self.TongueName(p.firstTongue) : "";
                string land = Homelands.TryGetValue(p.id, out string own) ? own : p.name;
                options.Add((land, (port != null ? $"Your home is {port.name}." : "") + (tongue.Length > 0 ? $" Your tongue is {tongue}." : "")));
            }
            int pick = 0;
            if (options.Count > 0)
                yield return Ask(AskPeople, options, i => pick = i);
            var people = peoples.Count > 0 ? peoples[Mathf.Clamp(pick, 0, peoples.Count - 1)] : null;
            PickedPeople = people != null ? people.id : "";
            if (people != null)
                MapPalette.SetPeople(people.id);

            yield return AskTheName(PickedPeople);

            var faiths = people != null ? book.FaithsFor(people) : new List<FaithDef>();
            options.Clear();
            foreach (var f in faiths)
                options.Add((f.name, FrameRoot.FaithLine(f)));
            pick = 0;
            if (options.Count > 1)
                yield return Ask(AskFaith, options, i => pick = i);
            PickedFaith = faiths.Count > 0 ? faiths[Mathf.Clamp(pick, 0, faiths.Count - 1)].id : "";

            options.Clear();
            foreach (var p in book.pasts)
                options.Add((p.name, FrameRoot.PastLine(book, p)));
            pick = 0;
            if (options.Count > 1)
                yield return Ask(AskPast, options, i => pick = i);
            PickedPast = book.pasts.Count > 0 ? book.pasts[Mathf.Clamp(pick, 0, book.pasts.Count - 1)].id : "";
        }

        /// <summary>The game begins behind the purple, with what was answered: who the merchant is, the lender's flag, the
        /// first jobs (one man aboard, a worn sail).</summary>
        public void OpeningBegin()
        {
            // the game begins behind the purple, with what was answered
            if (ContentFlags.Ready)
                ContentFlags.Set(Trade.LenderFlag);
            else
                flagOwed = true;                                         // (set as soon as the cards' state is there)
            Self.Call(PickedName);
            Self.SetFace(PickedFace);
            var frame = FrameRoot.Current;
            if (frame != null)
                frame.BeginSelfAs(PickedPeople, PickedFaith, PickedPast);
            else
                Self.Begin(PickedPeople, PickedFaith, PickedPast);
            Self.Call(PickedName);
            Self.SetFace(PickedFace);
            var world = ChartWorld.Current;
            var game = KapmanGame.Current;
            FirstJobs.Begin(world != null ? world.homePort : "", PickedPeople, game != null ? game.worldSeed : 0);
            if (PickedName.Length > 0)
                FrameLog.Add($"You are {PickedName}. One man is aboard, Ravn. The sail is worn, and she wants a crew.");
        }

        /// <summary>It is shown who asked: the lender steps out of the dark under the balloon's tail and says his lines.</summary>
        public IEnumerator OpeningLender()
        {
            var world = ChartWorld.Current;
            // and it is shown who asked
            Stage = "lender";
            face.Who = "lender";
            who.text = Trade.LenderName;                                    // (his byname is his own to say, in his second line)
            Show(speaker, 0f);
            StartCoroutine(Fade(a => speaker.style.opacity = a, 0f, 1f, 0.9f));   // (he steps out of the dark under the tail)
            var home = GameData.Port(world != null ? world.homePort : null);
            foreach (string line in LenderLines)
                yield return Say(Fill(line, home), true);
        }

        /// <summary>Through black to the showroom: the boat turning in a pink void, her plate, what is owed for her; a
        /// click goes on, through black again.</summary>
        public IEnumerator OpeningShowroom()
        {
            // the showroom
            yield return Fade(Black, 0f, 1f, 0.5f);
            Hide(box);
            Hide(backdrop);
            var hook = ShipStageHook.Current;
            if (hook != null)
            {
                Stage = "showroom";
                ShipStage.ShowroomVoid = new Color(0.86f, 0.42f, 0.66f);
                hook.ShowroomLines = 0;
                hook.Showroom = true;
                Show(shipFull);
                plate.Clear();
                foreach (string line in PlateLines)
                    plate.Add(Text("show-plate-line", line));
                string pounds = Pounds();
                var owed = Text("show-plate-quiet", $"{char.ToUpperInvariant(pounds[0]) + pounds.Substring(1)} pounds of silver, owed to {Trade.LenderByname}. Enter or a click to go on.");
                owed.style.fontSize = Mathf.Max(18f, H * 0.021f);
                plate.Add(owed);
                Show(plate);
                Show(veil, 0f);                                          // (a click anywhere goes on)
                yield return Wait(0.8f);
                yield return Fade(Black, 1f, 0f, 0.8f);
                yield return Wait(1.2f);
                clicked = false;
                while (!Go())
                    yield return null;
                yield return Fade(Black, 0f, 1f, 0.5f);
                Hide(veil);
                Hide(plate);
                Hide(shipFull);
                hook.Showroom = false;
            }
        }

        /// <summary>The first morning: the game is given back (the clock runs, the windows answer), the card of the place
        /// and the day comes over the black, and the next click is armed.</summary>
        public IEnumerator OpeningFirstMorning()
        {
            // the first morning
            Stage = "first morning";
            GameSpeed.Release(Hold_);
            UiFocus.Release(this);
            yield return null;
            var here = FrameGame.PortHere;
            lastPort = here != null ? here.id : "";
            before = false;                                              // (the title's music lets go; the game's own may come)
            yield return PlaceCard(here != null ? here.name : "At sea", DayWords(), true);

            // free, it seems: the next click is answered by somebody else
            Stage = "free";
            firstClicked = false;
            armed = true;
            Show(veil, 0f);
            busy--;
        }

        /// <summary>The first click or key of the first morning is taken (at once: nothing else answers it).</summary>
        void FirstClick(Vector2 at)
        {
            if (firstClicked)
                return;
            armed = false;
            busy++;
            Stage = "marks";
            firstClickAt = at;
            firstClicked = true;
        }

        /// <summary>Waits for the first click or key of the first morning.</summary>
        public IEnumerator OpeningFirstClick()
        {
            while (!firstClicked)
                yield return null;
        }

        string Fill(string line, PortDef home)
        {
            return line.Replace("{name}", PickedName.Length > 0 ? PickedName : "You")
                       .Replace("{lender}", Trade.LenderName)
                       .Replace("{byname}", Trade.LenderByname)
                       .Replace("{home}", home != null ? home.name : "home")
                       .Replace("{pounds}", Pounds())
                       .Replace("{pence}", Trade.Debt.ToString("#,0", CultureInfo.InvariantCulture))
                       .Replace("{day}", string.IsNullOrEmpty(Trade.DebtDueName) ? "the day" : Trade.DebtDueName);
        }

        static string Pounds()
        {
            double pounds = Trade.Debt / 240.0;
            int whole = (int)Math.Round(pounds);
            string[] words = { "no", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve" };
            return Math.Abs(pounds - whole) < 0.01 && whole >= 0 && whole < words.Length ? words[whole] : pounds.ToString("0.#", CultureInfo.InvariantCulture);
        }

        // ------------------------------------------------------------------ saying and asking in the box

        /// <summary>A line said a letter at a time in the box. A key or a click finishes it; `wait`: then another goes on.</summary>
        IEnumerator Say(string line, bool wait)
        {
            // POKEMON PACING (9 Oct 2026): a line that is waited on is said a box at a time, a sentence or two, the key between
            if (wait)
            {
                var boxes = TextReveal.Pages(line, TextReveal.PageChars);
                if (boxes.Count > 1)
                {
                    foreach (string box in boxes)
                        yield return Say(box, true);
                    yield break;
                }
            }
            list.Clear();
            Hide(small);
            Hide(go);
            words.text = line;
            var reveal = new TextReveal();
            reveal.Add(words);
            reveal.StartAt(0);
            clicked = false;
            yield return null;
            while (!reveal.Done)
            {
                if (Go())
                    reveal.Finish();
                else
                    reveal.Tick(Time.unscaledDeltaTime);
                yield return null;
            }
            if (!wait)
                yield break;
            Show(go);
            yield return null;
            while (!Go())
                yield return null;
            Hide(go);
        }

        /// <summary>A question with its answers as buttons (a number key picks too); what each answer means stands under
        /// them while the pointer is on it.</summary>
        IEnumerator Ask(string question, List<(string name, string small)> answers, Action<int> picked)
        {
            yield return Say(question, false);
            int chosen = -1;
            small.text = "";
            Show(small);
            for (int i = 0; i < answers.Count; i++)
            {
                int index = i;
                // (an answer is laid out in Game/UI/Parts/ShowChoice.uxml)
                var b = FrameParts.Make<Button>(FrameParts.Current != null ? FrameParts.Current.showChoice : null, "choice");
                b.text = (i + 1) + ". " + answers[i].name;
                b.clicked += () => chosen = index;
                string means = answers[i].small ?? "";
                b.RegisterCallback<PointerEnterEvent>(_ => small.text = means);
                list.Add(b);
            }
            if (answers.Count > 0)
                small.text = answers[0].small ?? "";
            var kb = Keyboard.current;
            while (chosen < 0)
            {
                kb = Keyboard.current;
                if (kb != null)
                    for (int i = 0; i < answers.Count && i < 9; i++)
                        if (kb[Key.Digit1 + i].wasPressedThisFrame || kb[Key.Numpad1 + i].wasPressedThisFrame)
                            chosen = i;
                yield return null;
            }
            list.Clear();
            Hide(small);
            clicked = false;
            picked(chosen);
        }

        /// <summary>The faces there are to choose from on this machine, as art slots, in the files' order.</summary>
        public static List<string> FacePool()
        {
            var found = new List<string>();
            foreach (string tier in new[] { "final", "standin" })
            {
                string dir = Path.Combine(FrameArt.Root, tier, "portrait_pool");
                if (!Directory.Exists(dir))
                    continue;
                foreach (string f in Directory.GetFiles(dir, "*.png"))
                {
                    string slot = "portrait_pool/" + Path.GetFileName(f);
                    if (!found.Contains(slot))
                        found.Add(slot);
                }
            }
            found.Sort(StringComparer.Ordinal);
            return found;
        }

        /// <summary>The faces in rows in the box; a click, or the number of one of the first nine, takes it.</summary>
        IEnumerator AskTheFace(List<string> faces)
        {
            Stage = "face";
            yield return Say(AskFace, false);
            int chosen = -1;
            var grid = Plain("show-faces");
            grid.pickingMode = PickingMode.Position;
            for (int i = 0; i < faces.Count; i++)
            {
                int index = i;
                // (a face is laid out in Game/UI/Parts/ShowFace.uxml)
                var b = FrameParts.Make<Button>(FrameParts.Current != null ? FrameParts.Current.showFace : null, "face-pick");
                b.clicked += () => chosen = index;
                var picture = FrameParts.In<Image>(b, "picture");
                picture.image = FrameArt.Load(faces[i]);
                picture.scaleMode = ScaleMode.ScaleAndCrop;
                picture.pickingMode = PickingMode.Ignore;
                grid.Add(b);
            }
            list.Add(grid);
            while (chosen < 0)
            {
                var kb = Keyboard.current;
                if (kb != null)
                    for (int i = 0; i < faces.Count && i < 9; i++)
                        if (kb[Key.Digit1 + i].wasPressedThisFrame || kb[Key.Numpad1 + i].wasPressedThisFrame)
                            chosen = i;
                yield return null;
            }
            list.Clear();
            clicked = false;
            PickedFace = faces[chosen];
            Stage = "asking";
        }

        /// <summary>"And your name?": typed, or given from the names of the people picked; Enter takes it.</summary>
        IEnumerator AskTheName(string people)
        {
            yield return Say(AskName, false);
            bool done = false;
            // (the box and its three buttons are laid out in Game/UI/Parts/ShowName.uxml)
            var form = FrameParts.Make<VisualElement>(FrameParts.Current != null ? FrameParts.Current.showName : null, "name-form");
            var field = FrameParts.In<TextField>(form, "field");
            field.maxLength = 24;
            NameField = field;
            var buttons = FrameParts.In<VisualElement>(form, "buttons");
            buttons.pickingMode = PickingMode.Position;
            var take = FrameParts.In<Button>(buttons, "take");
            take.clicked += () => done = field.value.Trim().Length > 0;
            FrameParts.In<Button>(buttons, "man").clicked += () => field.value = PeriodNames.Pick(people, false, dice.NextDouble(), field.value);
            FrameParts.In<Button>(buttons, "woman").clicked += () => field.value = PeriodNames.Pick(people, true, dice.NextDouble(), field.value);
            list.Add(field);
            list.Add(buttons);
            small.text = "Type a name, or have one given: names of your people, of these years.";
            Show(small);
            field.Focus();
            while (!done)
            {
                take.SetEnabled(field.value.Trim().Length > 0);
                var kb = Keyboard.current;
                if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) && field.value.Trim().Length > 0)
                    done = true;
                yield return null;
            }
            PickedName = field.value.Trim();
            NameField = null;
            list.Clear();
            Hide(small);
            clicked = false;
        }

        // ------------------------------------------------------------------ a place's card

        /// <summary>The place's name and the day across the middle of the screen. `onBlack`: the black is closed, and
        /// lifts while the card stands.</summary>
        IEnumerator PlaceCard(string place, string day, bool onBlack)
        {
            if (!onBlack)
                Stage = "place";
            cardBig.text = place ?? "";
            cardSmall.text = day ?? "";
            Show(card, 0f);
            yield return Fade(a => card.style.opacity = a, 0f, 1f, 0.5f);
            yield return Wait(onBlack ? 1.5f : 1.8f);
            if (onBlack)
            {
                yield return Fade(Black, 1f, 0f, 1.2f);
                yield return Wait(0.7f);
            }
            yield return Fade(a => card.style.opacity = a, 1f, 0f, 0.8f);
            Hide(card);
            if (!onBlack && Stage == "place")
                Stage = "";
        }

        // ------------------------------------------------------------------ the first click: !!!, and Ravn

        /// <summary>Where Ravn's face stands when he speaks (the left of the foot of the frame), in the marks' own place on
        /// the screen: the marks fly to it.</summary>
        /// <summary>The marks are on their way down to Ravn now (for a test, a picture).</summary>
        public bool MarksFlying { get; private set; }

        Vector2 RavnPlace()
        {
            var frame = FrameRoot.Current;
            var who = frame != null && frame.Root != null ? frame.Root.Q<VisualElement>("who") : null;
            if (who != null && who.panel != null && who.worldBound.width > 1f)
            {
                var box = who.worldBound;
                return marks.WorldToLocal(new Vector2(box.xMin + box.width * 0.215f, box.yMin + box.height * 0.5f));
            }
            return new Vector2(W * 0.282f, H * 0.885f);
        }

        /// <summary>The marks: "!!!" pop up where the first click fell, shake, and fly down to where Ravn's face is about to
        /// stand.</summary>
        public IEnumerator OpeningMarks()
        {
            Vector2 at = firstClickAt;
            if (!firstClicked)
            {
                // (the marks with no click before them, as a graph may have it: the middle of the world, and the game held as a click holds it)
                at = new Vector2(W * 0.5f, H * 0.45f);
                armed = false;
                busy++;
            }
            firstClicked = false;
            Stage = "marks";
            marks.Clear();
            Show(marks);
            float h = H;
            var made = new List<VisualElement>();
            var turns = new List<float>();
            // THE MARKS ARE OF THE TITLE'S OWN MAKING (the user, 8 Oct 2026: "same with the !!!, make them the same colour and
            // style as the title screen"): the picture title/title.mark.png, painted as the name's letters are (the worn
            // faded green slab with its dark line and its shadow); with no picture, a plain mark in that green.
            var markPicture = FrameArt.Load("title/title.mark.png");
            for (int i = 0; i < 7; i++)
            {
                float tall = h * (0.11f + 0.07f * (float)dice.NextDouble());
                VisualElement m;
                if (markPicture != null && markPicture.height > 0)
                {
                    var drawn = new Image { image = markPicture, scaleMode = ScaleMode.StretchToFill, pickingMode = PickingMode.Ignore };
                    drawn.AddToClassList("show-mark-picture");
                    drawn.style.height = tall * 1.25f;
                    drawn.style.width = tall * 1.25f * markPicture.width / markPicture.height;
                    m = drawn;
                }
                else
                {
                    m = Text("show-mark", "!");
                    m.style.fontSize = tall;
                }
                m.style.left = Mathf.Clamp(at.x + ((float)dice.NextDouble() - 0.5f) * h * 0.34f, 20f, W - 60f);
                m.style.top = Mathf.Clamp(at.y - h * 0.08f + ((float)dice.NextDouble() - 0.5f) * h * 0.22f, 20f, H - 120f);
                float turn = ((float)dice.NextDouble() - 0.5f) * 30f;
                turns.Add(turn);
                m.style.rotate = new Rotate(new Angle(turn, AngleUnit.Degree));
                m.style.scale = new Scale(Vector3.zero);
                marks.Add(m);
                made.Add(m);
            }
            foreach (var m in made)
            {
                var mark = m;
                // (each pops: over its size and back, a tenth of a second after the last)
                yield return Fade(k => mark.style.scale = new Scale(Vector3.one * (k < 0.7f ? k / 0.7f * 1.3f : Mathf.Lerp(1.3f, 1f, (k - 0.7f) / 0.3f))), 0f, 1f, 0.09f);
            }
            // they shake where they stand (the user, 7 Oct 2026: "animated and shakey like old nintendo games"): not
            // smoothly, but in steps, sixteen a second, each a jump to a new place and a new lean, with a hop now and then
            float shaken = 0f, step = 0f, amp = h * 0.012f;
            int beat = 0;
            while (shaken < MarksShake)
            {
                shaken += Time.unscaledDeltaTime;
                step -= Time.unscaledDeltaTime;
                if (step <= 0f)
                {
                    step = 1f / 16f;
                    beat++;
                    for (int i = 0; i < made.Count; i++)
                    {
                        float hop = (beat + i * 2) % 6 == 0 ? -amp * 2.2f : 0f;
                        made[i].style.translate = new Translate(((float)dice.NextDouble() - 0.5f) * 2f * amp, ((float)dice.NextDouble() - 0.5f) * 2f * amp + hop);
                        made[i].style.rotate = new Rotate(new Angle(turns[i] + ((float)dice.NextDouble() - 0.5f) * 16f, AngleUnit.Degree));
                        made[i].style.scale = new Scale(Vector3.one * (beat % 2 == 0 ? 1.08f : 0.96f));
                    }
                }
                yield return null;
            }
            // AND THEY GO DOWN TO RAVN (the user, 8 Oct 2026: "they start out like that and need to travel back down to
            // Ravn's portrait"): one after another each leaves its place and flies to where his face is about to
            // stand, in the foot of the frame, growing small as it goes. There it is gone; and then he speaks.
            Vector2 to = RavnPlace();
            var fromLeft = new List<float>();
            var fromTop = new List<float>();
            foreach (var m in made)
            {
                fromLeft.Add(m.resolvedStyle.left);
                fromTop.Add(m.resolvedStyle.top);
            }
            const float flight = 0.42f, apart = 0.06f;
            float flown = 0f, whole = flight + apart * Mathf.Max(0, made.Count - 1);
            MarksFlying = true;
            while (flown < whole)
            {
                flown += Time.unscaledDeltaTime;
                for (int i = 0; i < made.Count; i++)
                {
                    float k = Mathf.Clamp01((flown - apart * i) / flight);
                    if (k <= 0f)
                        continue;
                    var m = made[i];
                    if (k >= 1f)
                    {
                        Hide(m);
                        continue;
                    }
                    float go = k * k;                                     // (slow away, fast in)
                    float w = m.resolvedStyle.width, tall = m.resolvedStyle.height;
                    m.style.translate = new Translate(0f, 0f);
                    m.style.left = Mathf.Lerp(fromLeft[i], to.x - w * 0.5f, go);
                    m.style.top = Mathf.Lerp(fromTop[i], to.y - tall * 0.5f, go);
                    m.style.rotate = new Rotate(new Angle(Mathf.Lerp(turns[i], 0f, go), AngleUnit.Degree));
                    m.style.scale = new Scale(Vector3.one * Mathf.Lerp(1f, 0.3f, go));
                }
                yield return null;
            }
            MarksFlying = false;
            marks.style.visibility = StyleKeyword.Null;
            marks.Clear();
            marks.style.opacity = StyleKeyword.Null;
            Hide(marks);
            Hide(veil);
            Stage = "";
            busy--;
        }

        /// <summary>Ravn's first talk: who he is, how the game is worked, what must be done before she sails, and one
        /// good buy. In the frame's own floating window, said a letter at a time like any talk.</summary>
        public void RavnSpeaks()
        {
            var frame = FrameRoot.Current;
            if (frame == null || frame.Root == null)
                return;
            var spec = new FloatSpec
            {
                id = "ravn.first", title = "Ravn", who = RavnWho, face = "Ravn", speaker = "Ravn",
                speakerLine = "Your first shipman", about = RavnIs, mustAnswer = true, holdsClock = true,
            };
            string you = Self.Called.Length > 0 ? Self.Called : "you";
            // WHAT HE SPEAKS OF IS POINTED AT (the user, 8 Oct 2026: "when Ravn is explaining what to do and how to play can
            // you highlight what he's talking about"): [[>names]] in a line puts the frame's pointer round those parts
            // of the screen from those words on (FloatSpec.Say; the names are the parts' names in Game/UI/Frame.uxml).
            // RAVN'S FIRST MORNING, CUT DOWN (the script critic's report of 8 Oct 2026, taken up by the user). He is himself, then the
            // sail, the men, the wine and his one word on what to buy; the paragraph that read out the buttons and the clock is gone
            // ("Space stops it", from a Northman of 793), and so is the saying over of the five jobs, which the list on the screen
            // holds. Prices are said in words (Spoken). AT EASE HE SHORTENS HIS WORDS, the only one in the game who does (the report:
            // "a lighter rural Ravn who alone shortens words ... and says every word whole when he is in earnest").
            spec.Say($"There you are! {char.ToUpperInvariant(you[0]) + you.Substring(1)}, is it? Ravn. I come with the boat. He didn't say? No. He wouldn't.");
            spec.Say("He was eating, I'll wager. He's always eating. [[>ship]]I've been over her for you, keel to masthead. She floats! That's the good news. That's most of the good news.");
            if (FirstJobs.Active && FirstJobs.SailWorn)
                spec.Say($"[[>job-sail]]The sail's rotten along the foot. The first real wind would have it off her. Have it mended before we put out. [[>mend-sail]]{Spoken.Capital(Spoken.Pennies(FirstJobs.MendCost))} and {(FirstJobs.MendHours == 6.0 ? "half a day" : Spoken.Number(FirstJobs.MendHours) + " hours")}. I'd sew it myself, but you've not seen my sewing. Keep it so.");
            if (FirstJobs.Active && FirstJobs.HandsShort > 0)
                spec.Say($"And two of us can't row six oars. I can hardly row one. She wants {Spoken.Number(FirstJobs.HandsShort)} more shipmen. [[>verb-alehouse]]They sit in the alehouse. [[>figure-silver]]The hiring silver is {Spoken.Pennies(FirstJobs.HandMoney)} a man.");
            // THE WINE IS SAID ONCE (the critic's play of 8 Oct 2026, B3: "whose is the wine, and whose is the silver?"): it is the
            // player's to sell and is owed for with the boat. Until then the log said of every cask sold that what it fetched
            // was "his stake and not your gain", which read as if the silver were not the player's to spend.
            if (Trade.Started && Trade.Hold.Owed(ChartWorld.CountsGood) > 0)
                spec.Say("[[>verb-hold]]The wine in her is yours to sell, mind. He says mine. He says that of everything. You owe him for it all the same, so sell it dear.");
            string tip = FirstTip(out string tipGood, out string tipPort);
            FirstJobs.Tip(tipGood, tipPort);                              // (his word stands on the list, in the market and by the port)
            if (tip.Length > 0)
                spec.Say("[[>verb-trade]]" + tip);
            // A STAKE WITH A DAY (the alpha's row 17; 8 Oct 2026): the first order, which the game asks about when he
            // has done. A PLACEHOLDER LINE in his voice, the user's to write. Whoever wants it is the port's own
            // cellarer where its page has one.
            if (FirstJobs.FirstOrder(out var orderPort, out var orderGood, out int orderUnits, out double orderDays, out _))
            {
                var wants = PlaceBook.Current.People(orderPort.id, KapmanGame.Current != null ? KapmanGame.Current.Year : 793).Find(p => p.role == "cellarer");
                string who = wants != null ? $"{wants.name} at {orderPort.name}, the bishop's {wants.Role}," : $"They at {orderPort.name}";
                spec.Say($"[[>verb-errands]]One more. {who} wants {Spoken.Number(orderUnits)} of those {orderGood.unit.Split(' ', ',', '(')[0]}s inside {Spoken.Number(orderDays)} days. Pays over the market. Say yes or no when I've done.");
            }
            // (what he has said stands on the screen when he stops: the list, FrameRoot.Jobs.cs. He points at it and does not say it over)
            spec.Say("[[>jobs]]It's all there, look. I forget things. That's how I know where it's kept.");
            spec.options.Add(new FloatOption { label = "Go on", isDefault = true, pick = () => RavnDone(frame) });
            spec.shut = () => RavnDone(frame);
            ravnUp = true;
            frame.TodoShow(true);                                         // (he points at the list: its tab is the one shown)
            frame.OpenFloat(spec);
            FrameLog.Add("Ravn has a good deal to say.");
        }

        void RavnDone(FrameRoot frame)
        {
            ravnUp = false;
            frame.CloseFloat("ravn.first");
            FirstJobs.OfferFirstOrder();                                  // (the order he spoke of: the game asks, take it or refuse it)
        }

        /// <summary>
        /// One good buy for the first voyage, from the markets as they stand: a good the home market has on offer that
        /// fits what room and silver she has, and the port within reach that pays most over its price here, the nearer
        /// the better. "" when there is nothing worth saying.
        /// </summary>
        public static string FirstTip() => FirstTip(out _, out _);

        /// <summary>The same, with the good and the port it names ("" where there is nothing worth saying).</summary>
        public static string FirstTip(out string tipGood, out string tipPort)
        {
            tipGood = tipPort = "";
            var game = KapmanGame.Current;
            var home = FrameGame.PortHere;
            if (game == null || home == null || !Trade.Started)
                return "";
            var here = Markets.Of(home, game.Year, game.Day);
            if (here == null)
                return "";
            int month = game.Month;
            double silver = FrameGame.Silver - FirstJobs.MendCost - FirstJobs.HandMoney * FirstJobs.HandsShort;
            var (hx, hz) = WorldFrame.ToWorld(home.lat, home.lon);
            MarketGood best = null;
            PortDef bestPort = null;
            double bestScore = 0.0, bestBuy = 0.0, bestSell = 0.0;
            foreach (var g in here.Goods)
            {
                if (g == null || g.good == null || g.stock < 1f)
                    continue;
                double buy = here.Asks(g, month);
                if (buy <= 0.0 || buy > Math.Max(silver, 1.0) || g.good.Space > Trade.Hold.Free)
                    continue;
                int units = (int)Math.Min(Math.Min(Math.Floor(silver / buy), Trade.Hold.RoomFor(g.good)), Math.Floor(g.stock));
                if (units < 1)
                    continue;
                foreach (var port in GameData.Ports)
                {
                    if (port == null || port.id == home.id)
                        continue;
                    var (x, z) = WorldFrame.ToWorld(port.lat, port.lon);
                    double km = Math.Sqrt((x - hx) * (x - hx) + (z - hz) * (z - hz)) / 1000.0;
                    if (km > 320.0)
                        continue;                                        // (a first voyage: not across a sea)
                    var there = Markets.Of(port, game.Year, game.Day);
                    var wanted = there != null ? there.Get(g.good.id) : null;
                    if (wanted == null)
                        continue;
                    double sell = there.Pays(wanted, month);
                    double score = (sell - buy) * units / (1.0 + km / 120.0);
                    if (sell > buy * 1.15 && score > bestScore)
                    {
                        bestScore = score;
                        best = g;
                        bestPort = port;
                        bestBuy = buy;
                        bestSell = sell;
                    }
                }
            }
            if (best == null)
                return "";
            tipGood = best.good.id;
            tipPort = bestPort.id;
            string name = best.good.name.ToLowerInvariant();
            return $"One thing I do know. They ask about {Spoken.Pennies(bestBuy)} here for {name}, and at {bestPort.name} they pay about {Spoken.Number(bestSell)}. Buy what she'll hold and you can pay for, and we take it there.";
        }
    }

    /// <summary>One line of a scene told over the black (Showtime.Scene): its words; who says them (the name shown; ""
    /// for the teller, who has no face), whose face (an id or a name) and its picture where it is known; whether the
    /// line breaks off; and whether it is a hit.</summary>
    public sealed class ToldLine
    {
        public string text = "", who = "", face = "", slot = "";
        public bool cut;
        /// <summary>A hit: as this line comes up, a blood-red filter flashes over the screen and the picture is shaken,
        /// in hard steps as an old console did it.</summary>
        public bool hit;
    }

    /// <summary>The tail of the speech balloon: a point hanging from its lower edge, in the balloon's own colour with
    /// its own dark line down both sides (and none across the top, where it joins). In a UI file: BalloonTail.</summary>
    [UxmlElement]
    public sealed partial class BalloonTail : VisualElement
    {
        public Color fill = new Color32(203, 213, 189, 255), edge = new Color32(46, 18, 51, 255);   // (the balloon's ground: MainMenu.uss .show-box)

        public BalloonTail()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Paint;
        }

        void Paint(MeshGenerationContext ctx)
        {
            var r = contentRect;
            if (r.width < 2f || r.height < 2f)
                return;
            var p = ctx.painter2D;
            Vector2 a = new Vector2(0f, 0f), b = new Vector2(r.width, 0f), tip = new Vector2(r.width * 0.12f, r.height);
            p.fillColor = fill;
            p.BeginPath();
            p.MoveTo(a);
            p.LineTo(b);
            p.LineTo(tip);
            p.ClosePath();
            p.Fill();
            p.strokeColor = edge;
            p.lineWidth = 3f;
            p.BeginPath();
            p.MoveTo(a);
            p.LineTo(tip);
            p.LineTo(b);
            p.Stroke();
        }
    }
}
