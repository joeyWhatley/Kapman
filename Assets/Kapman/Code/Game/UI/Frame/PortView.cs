using Kapman.Game.ShipView;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE PORT'S PICTURE IN THE CHART'S PLACE (the user, 7 Oct 2026: "when you're at a port, instead of a map it just
    /// shows an artwork of the current port ... a top down angle looking at the place, not animated except for
    /// lighting ... simple psx graphics, 3D with 2D elements as it's just looking at one angle"). While she lies at a
    /// port, the port's picture from above (the art slot port_view/portview.&lt;port&gt;.png: the user's own when it is
    /// drawn, else the placeholder made by rule from the port's kind, size and landing) fills the screen behind the
    /// frame, where the chart is at sea. Nothing in it moves. The hour is laid over it as a tint, by the Sun's height
    /// where the port lies: day as drawn, warm toward evening, rose at dusk, blue and dark at night.
    ///
    /// A small button at the top of the picture puts the chart back while she lies there (a course is still set in the
    /// Voyage window either way), and the port's picture again. A port with no picture keeps the chart.
    /// Not shown while the ship's 3D view has the middle of the screen. Off in a batch run with the rest of the show
    /// (the tests click on the chart in port) unless a test asks for it: Showtime.On.
    /// </summary>
    public sealed class PortView : MonoBehaviour
    {
        public static PortView Current { get; private set; }

        /// <summary>The player has asked for the chart while in port (kept until she casts off).</summary>
        public bool ChartAsked { get; private set; }
        /// <summary>The picture is up now, and of which port.</summary>
        public bool Shown { get; private set; }
        public string Port { get; private set; } = "";
        public Image Picture => picture;
        public Button Flip => flip;
        /// <summary>The Sun's height set by hand, in degrees (a picture, a test); null: where the Sun is.</summary>
        public static float? SunOverride;
        /// <summary>The tint laid over the picture now.</summary>
        public Color Light { get; private set; } = Color.white;

        Image picture;
        Button flip;
        Label ground;                                                 // a place's name on a plain ground, where it has no picture yet
        FrameRoot framed;
        Button wiredFlip;                                             // the button whose click is ours already
        readonly System.Collections.Generic.List<VisualElement> made = new System.Collections.Generic.List<VisualElement>();   // parts made here, not the frame's file's
        string loadedFor = "";
        Texture2D texture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = null;
            SunOverride = null;
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
            new GameObject("Port view").AddComponent<PortView>();
        }

        void OnDestroy()
        {
            foreach (var m in made)
                m.RemoveFromHierarchy();                                 // (the frame's own parts stay)
            made.Clear();
            if (Current == this)
                Current = null;
        }

        /// <summary>The slot of a port's picture from above.</summary>
        public static string Slot(string portId) => "port_view/portview." + portId + ".png";

        /// <summary>
        /// The tint of an hour, by the Sun's height in degrees: as drawn by day, warm as it sinks, rose about the
        /// horizon, blue through the dusk and dark blue at night (never black: the place must still be read).
        /// </summary>
        public static Color LightAt(float sunAltitude)
        {
            Color day = Color.white, low = new Color(1f, 0.88f, 0.72f), set = new Color(0.86f, 0.58f, 0.56f),
                  dusk = new Color(0.42f, 0.40f, 0.62f), night = new Color(0.20f, 0.23f, 0.40f);
            if (sunAltitude >= 14f)
                return day;
            if (sunAltitude >= 4f)
                return Color.Lerp(low, day, (sunAltitude - 4f) / 10f);
            if (sunAltitude >= -1f)
                return Color.Lerp(set, low, (sunAltitude + 1f) / 5f);
            if (sunAltitude >= -7f)
                return Color.Lerp(dusk, set, (sunAltitude + 7f) / 6f);
            if (sunAltitude >= -14f)
                return Color.Lerp(night, dusk, (sunAltitude + 14f) / 7f);
            return night;
        }

        void Update()
        {
            var frame = FrameRoot.Current;
            if (frame == null || frame.Root == null)
            {
                Shown = false;
                return;
            }
            if (frame != framed || picture == null || picture.panel == null)
            {
                // THE FRAME'S OWN PARTS (KAPMAN_PLAN step 2d, 14 Oct 2026): Game/UI/Frame.uxml holds the picture
                // "port-view" and the ground "place-ground" first under the frame's root, and the button "port-flip"
                // among the view buttons of the top bar (with the other buttons that change what is looked at: the
                // user's ruling of 7 Oct 2026). They are found by name and filled here; a frame without them (gone from
                // the file, or a test's bare frame) gets them made as before.
                foreach (var m in made)
                    m.RemoveFromHierarchy();
                made.Clear();
                picture = frame.Root.Q<Image>("port-view");
                if (picture == null)
                {
                    picture = new Image { name = "port-view" };
                    picture.AddToClassList("frame-port-view");
                    frame.Root.Insert(0, picture);
                    made.Add(picture);
                }
                picture.scaleMode = ScaleMode.ScaleAndCrop;
                ground = frame.Root.Q<Label>("place-ground");
                if (ground == null)
                {
                    ground = new Label { name = "place-ground", pickingMode = PickingMode.Ignore };
                    ground.AddToClassList("frame-port-view");
                    ground.AddToClassList("frame-place-ground");
                    frame.Root.Insert(1, ground);
                    made.Add(ground);
                }
                ground.style.display = DisplayStyle.None;
                flip = (frame.ViewButtons ?? frame.Root).Q<Button>("port-flip");
                if (flip == null)
                {
                    flip = new Button { name = "port-flip", text = "Chart", tooltip = "The chart in the port's place, and back." };
                    flip.AddToClassList("frame-k");
                    (frame.ViewButtons ?? frame.Root).Add(flip);
                    made.Add(flip);
                }
                if (flip != wiredFlip)
                {
                    flip.clicked += () => ChartAsked = !ChartAsked;
                    wiredFlip = flip;
                }
                framed = frame;
            }
            var here = FrameGame.PortHere;
            var menu = MainMenu.Current;
            var hook = ShipStageHook.Current;
            if (here == null)
                ChartAsked = false;                                      // (at sea the chart is the screen: the asking ends)
            bool can = Showtime.On && here != null && Self.Chosen && !(menu != null && menu.OnTitle) && !(hook != null && hook.Swapped);
            // A PLACE GONE INTO (FramePlace: the alehouse, the holy place, the fire on the bank) shows in the port's
            // stead: its own picture, or a plain ground with its name until the user has sent one.
            bool place = can && FramePlace.At != PlaceKind.None;
            string slot = !can ? "" : place ? FramePlace.Picture : Slot(here.id);
            if (can && loadedFor != slot)
            {
                loadedFor = slot;
                texture = FrameArt.Load(slot);
                picture.image = texture;
            }
            bool bare = place && texture == null;
            if (ground != null)
            {
                ground.style.display = bare ? DisplayStyle.Flex : DisplayStyle.None;
                if (bare)
                    ground.text = FramePlace.Name;
            }
            can &= texture != null || bare;
            Shown = can && (!ChartAsked || place);
            Port = Shown ? here.id : "";
            picture.style.display = Shown && texture != null ? DisplayStyle.Flex : DisplayStyle.None;
            flip.style.display = can && !place ? DisplayStyle.Flex : DisplayStyle.None;
            flip.text = ChartAsked ? "Port" : "Chart";
            if (!Shown)
                return;
            // the hour's light, by the Sun where the port lies
            var clock = LevelClock.Current;
            Color want = Color.white;
            if (SunOverride.HasValue)
                want = LightAt(SunOverride.Value);
            else if (clock != null)
                want = LightAt(StageSky.At(clock.JulianDay, here.lat, here.lon).sunAltitude);
            Light = Color.Lerp(Light, want, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 2f));
            picture.tintColor = Light;
        }
    }
}
