using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE PARTS THE FRAME MAKES AGAIN AND AGAIN, EACH A UI FILE (KAPMAN, step 2; the user, 7 Oct 2026: "make it like
    /// people usually make it, not just all in script"; by the picker: UI Builder files). A side window, the box of what
    /// is said, one choice, a line of a window, a line of the log: each is laid out in a file of Game/UI/Parts, and the
    /// code takes a copy of it and fills in the words (<see cref="Make{T}"/>, <see cref="In{T}"/>). This asset names the
    /// files. It lies in a Resources folder so that it is found without a scene (the tests fill windows with none).
    /// The menu's choice and the opening's screens and parts are named here too. The rule: a whole screen is the Source
    /// Asset of its UI Document (Frame.uxml, MainMenu.uxml); a part the code takes copies of is named here.
    /// </summary>
    [CreateAssetMenu(menuName = "Kapman/Frame parts", fileName = "FrameParts")]
    public sealed class FrameParts : ScriptableObject
    {
        public const string ResourceName = "FrameParts";

        [Tooltip("A side window: its head and the scroll its panel is put in (Game/UI/Parts/Window.uxml, the element \"window\").")]
        public VisualTreeAsset window;
        [Tooltip("What is said, and a pop-up window (Game/UI/Parts/Float.uxml, the element \"float\").")]
        public VisualTreeAsset floatBox;
        [Tooltip("One choice of a talk or a card (Game/UI/Parts/Option.uxml, the element \"option\").")]
        public VisualTreeAsset option;
        [Tooltip("A line of a window (Game/UI/Parts/Line.uxml, the element \"line\").")]
        public VisualTreeAsset line;
        [Tooltip("A line that is a button (Game/UI/Parts/LineButton.uxml, the element \"line\").")]
        public VisualTreeAsset lineButton;
        [Tooltip("A line of the log (Game/UI/Parts/LogLine.uxml, the element \"log-line\").")]
        public VisualTreeAsset logLine;
        [Tooltip("The box that comes up over a line of the log: its heading and one row (Game/UI/Parts/HoverBox.uxml, the elements \"head\" and \"box-row\").")]
        public VisualTreeAsset hoverBox;
        [Tooltip("A knot of her course on the chart: a point the course has been pulled through (Game/UI/Parts/CourseKnot.uxml, the element \"knot\").")]
        public VisualTreeAsset courseKnot;
        [Tooltip("The helm's mark at the foot of the 3D view: whose hand, the tiller, the keys (Game/UI/Parts/HelmMark.uxml, the element \"helm\").")]
        public VisualTreeAsset helmMark;
        [Tooltip("One good of a market's table (Game/UI/Parts/MarketRow.uxml, the element \"item\").")]
        public VisualTreeAsset marketRow;
        [Tooltip("One option of the text panel (Game/UI/Parts/TextOption.uxml, the button \"option\").")]
        public VisualTreeAsset textOption;
        [Tooltip("One notice at the top of the screen (Game/UI/Parts/Notice.uxml, the label \"notice\").")]
        public VisualTreeAsset notice;
        [Tooltip("The picker of the notes of F2: its veil, its mark and its hint (Game/UI/Parts/NotePicker.uxml, the elements \"note-veil\", \"note-mark\", \"note-hint\").")]
        public VisualTreeAsset notePicker;
        [Tooltip("The slip of a note of F2 (Game/UI/Parts/NoteSlip.uxml, the element \"note-slip\").")]
        public VisualTreeAsset noteSlip;

        [Header("The side windows' own layouts")]
        [Tooltip("One file a panel, named by the panel's id (Game/UI/Panels/<id>.uxml, the element \"panel\" with the panel's named sections). A panel without one fills its window's body as it always did.")]
        public VisualTreeAsset[] panels = new VisualTreeAsset[0];

        /// <summary>The window file of the panel with this id, or null when it has none.</summary>
        public static VisualTreeAsset Panel(string id)
        {
            var c = Current;
            if (c == null || c.panels == null || string.IsNullOrEmpty(id))
                return null;
            foreach (var v in c.panels)
                if (v != null && v.name == id)
                    return v;
            return null;
        }

        [Header("The menu and the opening")]
        [Tooltip("One choice of the menu (Game/UI/Parts/MenuOption.uxml, the element \"option\").")]
        public VisualTreeAsset menuOption;
        [Tooltip("The opening's screens, two layers (Game/UI/Showtime.uxml: \"show-under\" and \"show-over\").")]
        public VisualTreeAsset showtime;
        [Tooltip("One answer of the opening (Game/UI/Parts/ShowChoice.uxml, the element \"choice\").")]
        public VisualTreeAsset showChoice;
        [Tooltip("One face to choose from (Game/UI/Parts/ShowFace.uxml, the element \"face-pick\").")]
        public VisualTreeAsset showFace;
        [Tooltip("The name's box and its buttons (Game/UI/Parts/ShowName.uxml, the element \"name-form\").")]
        public VisualTreeAsset showName;

        static FrameParts current;
        static bool looked;

        /// <summary>The parts (Resources/FrameParts), or null if the asset is gone: that is said once in the Console.</summary>
        public static FrameParts Current
        {
            get
            {
                if (current == null && !looked)
                {
                    looked = true;
                    current = Resources.Load<FrameParts>(ResourceName);
                    if (current == null)
                        Debug.LogError("Kapman frame: no Resources/" + ResourceName + " (Kapman > Make physical > 2. The frame's layout files): its windows and lines have no layout.");
                }
                return current;
            }
        }

        /// <summary>A fresh copy of a part: the element named `partName` of a layout file, without the file's own
        /// wrapper. If the file or the element is gone, that is said in the Console and a plain one is made.</summary>
        public static T Make<T>(VisualTreeAsset layout, string partName) where T : VisualElement, new()
        {
            T made = layout != null ? layout.Instantiate().Q<T>(partName) : null;
            if (made == null)
            {
                Debug.LogError("Kapman frame: " + (layout != null ? "the layout file " + layout.name + ".uxml has no " + typeof(T).Name + " named \"" + partName + "\""
                                   : "the layout file of the part \"" + partName + "\" is not set in Resources/" + ResourceName) + ". A plain one is made.");
                return new T { name = partName };
            }
            made.RemoveFromHierarchy();
            return made;
        }

        /// <summary>A named part inside a copy. If the file has none such (taken out or renamed in UI Builder), that is
        /// said in the Console and a plain one is made at its end, so the game still runs.</summary>
        public static T In<T>(VisualElement within, string partName) where T : VisualElement, new()
        {
            var e = within.Q<T>(partName);
            if (e != null)
                return e;
            Debug.LogError("Kapman screen: \"" + within.name + "\" has no " + typeof(T).Name + " named \"" + partName +
                           "\" in its UI file. A plain one is made: put it back in UI Builder, under that name.");
            e = new T { name = partName };
            within.Add(e);
            return e;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            current = null;
            looked = false;
        }
    }
}
