// PATH IN THE PROJECT: Assets/Kapman/Code/Game/ShipView/ShipStageHook.cs   (assembly Kapman.Game)
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Game.rsp; never run. Not checked by any compiler:
// that UIDocument.sortingOrder re-sorts a document at run time, and which way up a camera's texture comes out in an Image.
using Kapman.Game.Frame;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE 3D VIEW IN THE FRAME, WITH NO LINE CHANGED IN THE FRAME. It starts itself in the chart scene (a scene with a
    /// ChartWorld) when the kit is there (Resources/ShipStageKit), makes the stage, and hangs three things on the frame's
    /// own elements, found by their classes:
    ///   - in the ship window (FrameRoot's "frame-ship-view", where FrameShipView paints the drawn stand-in): a picture
    ///     that fills it and so covers the stand-in, showing the stage's texture; and at its top left the Swap button
    ///     (the user, 7 Oct 2026: "a simple button on the ... top left of the 3D sailing boat that switches place with the
    ///     map and miniaturises the map");
    ///   - behind every slip of the frame (first child of FrameRoot.Root): the same texture filling the screen, shown
    ///     when swapped.
    /// SWAPPED, the chart is not drawn into a texture: the map's own document is made small. Its root is given the ship
    /// window's rectangle and its sorting order is put just over the frame's (42.5: over the frame at 42, under the
    /// notices at 43 and the menu at 50), so the chart goes on working in the small window (it still pans, zooms and
    /// takes clicks) and a second Swap button on it brings it back. No camera changes its target: the chart's camera
    /// (which draws nothing) always has the screen and the stage's camera always draws to its texture; only the
    /// texture's size and which picture shows it change.
    /// The swap stands down while the frame itself is hidden (the title, the chest open on the map).
    /// With the kit missing, no graphics device, or <see cref="Off"/> set, nothing is made and the drawn stand-in stays.
    /// </summary>
    [DefaultExecutionOrder(950)]                                        // after the frame (900), before the stage (960) and the map (1000)
    public sealed class ShipStageHook : MonoBehaviour
    {
        /// <summary>Tests and tools: no 3D view (the drawn stand-in stays).</summary>
        public static bool Off;
        public static ShipStageHook Current { get; private set; }
        /// <summary>The 3D view fills the screen and the chart is in the small window.</summary>
        public bool Swapped;

        ShipStageKit kit;
        ShipStage stage;
        FrameRoot framed;
        VisualElement shipView, mapRoot;
        Image small, big;
        Button swapSmall, swapMini;
        UIDocument mapDoc;
        float mapOrder;
        bool applied;
        static bool warned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = null;
            Off = false;
            warned = false;
            SceneManager.sceneLoaded -= Loaded;
            SceneManager.sceneLoaded += Loaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void First() => TryStart();

        static void Loaded(Scene scene, LoadSceneMode mode) => TryStart();

        static void TryStart()
        {
            if (Off || Current != null || ChartWorld.Current == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                return;
            var kit = ShipStageKit.Load();
            if (kit == null)
            {
                if (!warned)
                    Debug.Log("Kapman ship view: no Resources/" + ShipStageKit.ResourceName + " (Kapman > Ship view > Make the kit): the ship window keeps its drawn stand-in.");
                warned = true;
                return;
            }
            var go = new GameObject("Ship view (3D)");
            Current = go.AddComponent<ShipStageHook>();
            Current.kit = kit;
        }

        void OnDestroy()
        {
            if (applied)
                Apply(false);
            Detach();
            if (stage != null)
                Destroy(stage.gameObject);
            if (Current == this)
                Current = null;
        }

        void Update()
        {
            var frame = FrameRoot.Current;
            if (frame == null || frame.Root == null)
            {
                if (stage != null)
                    stage.Running = false;
                return;
            }
            if (frame != framed || shipView == null || shipView.panel == null)
                Attach(frame);
            if (shipView == null)
                return;
            if (stage == null)
                stage = ShipStage.Build(kit, new ChartStageSource());
            bool shown = frame.Root.resolvedStyle.display != DisplayStyle.None;
            var map = MapView.Current;
            bool want = Swapped && shown && map != null && !map.ChestOpen;
            if (want != applied)
                Apply(want);
            stage.Running = shown;
            stage.Big = applied;
            Layout(frame);
        }

        /// <summary>Hangs the pictures and the button on the frame as it is built now (it builds itself anew when enabled).</summary>
        void Attach(FrameRoot frame)
        {
            Detach();
            framed = frame;
            shipView = frame.Root.Q(null, "frame-ship-view");
            if (shipView == null)
                return;
            small = Picture();
            shipView.Add(small);
            swapSmall = SwapButton("Put the ship in the middle of the screen and the chart in this window.");
            shipView.Add(swapSmall);
            big = Picture();
            big.style.display = DisplayStyle.None;
            frame.Root.Insert(0, big);
            applied = false;
        }

        void Detach()
        {
            small?.RemoveFromHierarchy();
            big?.RemoveFromHierarchy();
            swapSmall?.RemoveFromHierarchy();
            small = big = null;
            swapSmall = null;
            shipView = null;
            framed = null;
        }

        static Image Picture()
        {
            var i = new Image { scaleMode = ScaleMode.ScaleAndCrop, pickingMode = PickingMode.Ignore };
            i.style.position = Position.Absolute;
            i.style.left = 0;
            i.style.top = 0;
            i.style.right = 0;
            i.style.bottom = 0;
            return i;
        }

        /// <summary>The button, dressed here and not in a style sheet (Frame.uss is the frame builder's; and one of the two
        /// sits on the map's document, which has none of the frame's styles).</summary>
        Button SwapButton(string tip)
        {
            var b = new Button(() => Swapped = !Swapped) { text = "Swap", tooltip = tip };
            b.style.position = Position.Absolute;
            b.style.left = 6;
            b.style.top = 6;
            b.style.paddingLeft = b.style.paddingRight = 8;
            b.style.paddingTop = b.style.paddingBottom = 2;
            b.style.marginLeft = b.style.marginRight = b.style.marginTop = b.style.marginBottom = 0;
            b.style.fontSize = 14;
            b.style.color = (Color)new Color32(236, 235, 230, 255);
            b.style.backgroundColor = (Color)new Color32(38, 26, 44, 235);
            b.style.borderLeftWidth = b.style.borderRightWidth = b.style.borderTopWidth = b.style.borderBottomWidth = 2;
            Color edge = new Color32(140, 106, 142, 255);
            b.style.borderLeftColor = b.style.borderRightColor = b.style.borderTopColor = b.style.borderBottomColor = edge;
            b.style.borderTopLeftRadius = b.style.borderTopRightRadius = b.style.borderBottomLeftRadius = b.style.borderBottomRightRadius = 8;
            return b;
        }

        /// <summary>Changes the places of the chart and the 3D view (on), or puts them back (off).</summary>
        void Apply(bool on)
        {
            var map = MapView.Current;
            if (on)
            {
                mapDoc = map != null ? map.GetComponent<UIDocument>() : null;
                var frameDoc = framed != null ? framed.GetComponent<UIDocument>() : null;
                if (mapDoc == null || frameDoc == null || mapDoc.rootVisualElement == null)
                    return;
                mapRoot = mapDoc.rootVisualElement;
                mapOrder = mapDoc.sortingOrder;
                mapDoc.sortingOrder = frameDoc.sortingOrder + 0.5f;
                mapRoot.style.overflow = Overflow.Hidden;
                mapRoot.style.borderTopLeftRadius = mapRoot.style.borderTopRightRadius = mapRoot.style.borderBottomLeftRadius = mapRoot.style.borderBottomRightRadius = 9;
                swapMini = SwapButton("Put the chart back in the middle of the screen.");
                mapRoot.Add(swapMini);
                if (big != null) big.style.display = DisplayStyle.Flex;
                if (small != null) small.style.display = DisplayStyle.None;
                if (swapSmall != null) swapSmall.style.display = DisplayStyle.None;
            }
            else
            {
                swapMini?.RemoveFromHierarchy();
                swapMini = null;
                if (mapDoc != null && mapRoot != null)
                {
                    mapDoc.sortingOrder = mapOrder;
                    mapRoot.style.left = 0;                               // (as MapView.OnEnable set them)
                    mapRoot.style.top = 0;
                    mapRoot.style.right = 0;
                    mapRoot.style.bottom = 0;
                    mapRoot.style.width = StyleKeyword.Auto;
                    mapRoot.style.height = StyleKeyword.Auto;
                    mapRoot.style.overflow = Overflow.Visible;
                    mapRoot.style.borderTopLeftRadius = mapRoot.style.borderTopRightRadius = mapRoot.style.borderBottomLeftRadius = mapRoot.style.borderBottomRightRadius = 0;
                }
                mapDoc = null;
                mapRoot = null;
                if (big != null) big.style.display = DisplayStyle.None;
                if (small != null) small.style.display = DisplayStyle.Flex;
                if (swapSmall != null) swapSmall.style.display = DisplayStyle.Flex;
            }
            applied = on;
            if (map != null)
                map.Follow();                                             // (the chart's view goes back to her at its own scale: the window changed size)
        }

        /// <summary>The texture's size for where it is shown now, and (swapped) the chart kept to the ship window's rectangle.</summary>
        void Layout(FrameRoot frame)
        {
            Rect all = frame.Root.worldBound, win = shipView.worldBound;
            if (all.width < 2f || win.width < 2f || win.height < 2f || float.IsNaN(win.width))
                return;
            var view = stage.View;
            int w, h;
            if (applied)
            {
                int ask = view.bigHeight > 0 ? view.bigHeight : kit.bigHeight;
                h = ask > 0 ? Mathf.Min(ask, Screen.height) : Screen.height;
                w = Mathf.RoundToInt(h * (all.width / all.height));
                view.every = 1;
                mapRoot.style.left = win.x;
                mapRoot.style.top = win.y;
                mapRoot.style.width = win.width;
                mapRoot.style.height = win.height;
                mapRoot.style.right = StyleKeyword.Auto;
                mapRoot.style.bottom = StyleKeyword.Auto;
            }
            else
            {
                h = view.smallHeight > 0 ? view.smallHeight : kit.smallHeight;
                w = Mathf.RoundToInt(h * (win.width / win.height));
                view.every = Mathf.Max(1, kit.smallEvery);
            }
            // the place it is shown in, in screen pixels (a look that makes its own picture sizes it from this)
            float px = Screen.width / all.width;
            if (applied)
                view.ShownAt(Screen.width, Screen.height);
            else
                view.ShownAt(Mathf.RoundToInt(win.width * px), Mathf.RoundToInt(win.height * px));
            bool made = !view.drawnByLook && view.Size(w, h);
            var picture = view.Picture;
            if (made || small.image != picture)
            {
                small.image = picture;
                big.image = picture;
                view.Render(true);                                        // (a new texture is empty until it is drawn)
            }
        }
    }
}
