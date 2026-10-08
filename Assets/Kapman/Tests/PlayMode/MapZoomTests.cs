using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE CHART'S ZOOM AND ITS FOG (9 Oct 2026; the user: "Can you fix the map zoom and the fog of war"). Held here, on
    /// the chart as a player has it, with the wheel and the pointer:
    ///   - the fog's picture lies on the world at every scale (it stood as it was first drawn: an Image is not drawn
    ///     again when only the part of its picture changes, and the fog's place on the screen never does);
    ///   - while she is followed she keeps her place on the screen as the wheel turns, and the view closes on its scale
    ///     over a few frames instead of jumping a notch;
    ///   - not followed, the place under the pointer stays under it;
    ///   - the view never leaves the painted chart, by the wheel or by the hand;
    ///   - the fog hides what she has not seen (the same day: "Why are we doing such a subtle fog? Shouldn't it be you
    ///     can't see anything you haven't been?"): far from home, land and sea are one dark; her home waters are not;
    ///   - the first morning's chart is home and the port nearest it, and the rest is the fog's dark (later that day:
    ///     "narrow the starting fog area, just Dorestead and the closest port"), and it opens closer ("it needs to be
    ///     more zoomed in otherwise the map is overwhelming and cluttered");
    ///   - a place is its dot. Its name comes up beside it only while the pointer is on the dot, stays a few seconds
    ///     after the pointer has left, and fades ("hide the name of somewhere unless you're hovering over the dot and
    ///     let it linger for a few seconds when the mouse is removed say 7 seconds then it fades away again to just a
    ///     dot"); near the chart's edge it stands on the other side of its dot; a water she knows is a mark with its
    ///     name under the pointer in the same way;
    ///   - scrolled on in past the closest, the ship's own view is asked for once.
    /// Pictures in Captures/chart (zoom_*.png).
    /// </summary>
    public class MapZoomTests
    {
        static Button Small(ChartPlay play, string words) =>
            play.Buttons(words, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-k"));

        /// <summary>The middle and the size of everything on the screen that is of one colour (a marked square of the fog).</summary>
        static bool Find(Color32[] px, int width, int height, Rect within, System.Func<Color32, bool> is_, out Vector2 middle, out Vector2 size)
        {
            long sx = 0, sy = 0, n = 0;
            int x0 = int.MaxValue, x1 = int.MinValue, y0 = int.MaxValue, y1 = int.MinValue;
            for (int y = Mathf.Max(0, (int)within.yMin); y < Mathf.Min(height, (int)within.yMax); y++)
                for (int x = Mathf.Max(0, (int)within.xMin); x < Mathf.Min(width, (int)within.xMax); x++)
                    if (is_(px[y * width + x]))
                    {
                        sx += x;
                        sy += y;
                        n++;
                        x0 = Mathf.Min(x0, x);
                        x1 = Mathf.Max(x1, x);
                        y0 = Mathf.Min(y0, y);
                        y1 = Mathf.Max(y1, y);
                    }
            middle = n > 0 ? new Vector2(sx / (float)n + 0.5f, sy / (float)n + 0.5f) : default;
            size = n > 0 ? new Vector2(x1 - x0 + 1, y1 - y0 + 1) : default;
            return n > 0;
        }

        /// <summary>The colour in the middle of a small square of the screen (the middle one of 81: a speck or a mark's
        /// edge does not count).</summary>
        static Color32 MiddleOf(Color32[] px, int width, int height, Vector2 at)
        {
            var r = new List<byte>();
            var g = new List<byte>();
            var b = new List<byte>();
            for (int y = (int)at.y - 4; y <= (int)at.y + 4; y++)
                for (int x = (int)at.x - 4; x <= (int)at.x + 4; x++)
                {
                    var c = px[Mathf.Clamp(y, 0, height - 1) * width + Mathf.Clamp(x, 0, width - 1)];
                    r.Add(c.r);
                    g.Add(c.g);
                    b.Add(c.b);
                }
            r.Sort();
            g.Sort();
            b.Sort();
            return new Color32(r[r.Count / 2], g[g.Count / 2], b[b.Count / 2], 255);
        }

        static int Apart(Color32 a, Color32 b) => Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Max(Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b)));

        /// <summary>How many names stand on the chart now.</summary>
        static int NamesUp(ChartPlay play)
        {
            int up = 0;
            play.Screen.Query<VisualElement>(className: "map-name").ForEach(n =>
            {
                if (ChartPlay.Shown(n))
                    up++;
            });
            return up;
        }

        /// <summary>The fog's picture drawn again from what is known (as a saved game does it: through its text).</summary>
        static void FogAgain() => Exploration.Restore(Json.Parse(Json.Write(Exploration.Capture())) as Dictionary<string, object>);

        static IEnumerator Wheel(ChartPlay play, Vector2 at, int notches, float settle = 0.9f)
        {
            for (int i = 0; i < Mathf.Abs(notches); i++)
            {
                play.WheelAt(at, notches > 0 ? 3f : -3f);
                yield return null;
            }
            yield return ChartPlay.Settle(settle);
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator TheChartZoomsAndItsFogGoesWithIt()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            ClockDrops.Quiet = true;
            ClockSettings.AsNew = true;
            string saves = Path.Combine(Application.temporaryCachePath, "kapman_mapzoom_saves");
            Directory.CreateDirectory(saves);
            foreach (string old in Directory.GetFiles(saves))
                File.Delete(old);
            SaveGame.FolderOverride = saves;
            var load = SceneManager.LoadSceneAsync("L_Chart", LoadSceneMode.Single);
            Assert.IsNotNull(load, "L_Chart is not in the build settings");
            while (!load.isDone)
                yield return null;
            yield return ChartPlay.Frames(5);
            MainMenu.Current.NewGame();
            SaveGame.FolderOverride = saves;
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            ClockDrops.Quiet = true;
            float from = Time.realtimeSinceStartup;
            while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                yield return null;
            var play = ChartPlay.Begin(false);
            int asked = 0;
            System.Action onPast = () => asked++;
            try
            {
                yield return ChartPlay.Frames(120);
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                from = Time.realtimeSinceStartup;
                while (FrameGame.PortHere == null && Time.realtimeSinceStartup - from < 15f)
                    yield return null;
                yield return ChartPlay.Frames(30);
                var view = PortView.Current;
                if (view != null && view.Shown)
                {
                    yield return play.Click(view.Flip, "Chart");
                    yield return ChartPlay.Frames(6);
                }
                yield return play.Click(Small(play, "Pause"), "Pause");   // (she lies still, and nothing comes up over the chart)
                var map = MapView.Current;
                Assert.IsNotNull(map, "no chart");
                Assert.IsTrue(map.chartMode && map.Framed, "the chart is not the game's screen behind the frame");
                yield return ChartPlay.Settle(1.2f);
                Assert.IsTrue(map.Painted(out double artWest, out double artNorth, out double artEast, out double artSouth), "the chart has no painted picture (MapArt)");
                Assert.IsTrue(Exploration.Ready, "no fog of war");
                Assert.IsTrue(map.Following, "a new game's chart does not follow her");
                Assert.AreEqual(map.chartScale, map.Scale, map.chartScale * 0.01, "the chart does not open at its own scale");
                var hole = play.Frame.Root.Q<VisualElement>("viewport").worldBound;
                var middle = hole.center;
                var ship = play.Screen.Q<VisualElement>(className: "map-ship");
                Assert.IsNotNull(ship, "no mark for her on the chart");
                var her = ChartWorld.ShipWorld();
                Vector2 Px(WorldXZ w) => map.ChartOverlays.LocalToWorld(map.ChartPx(w));
                Assert.Less((Px(her) - middle).magnitude, 6f, "followed, she is not in the middle of what the frame leaves free: " + Px(her) + " against " + middle);

                // ---- 0. THE FIRST MORNING (the user, 9 Oct 2026, later). THE CHART OPENS CLOSER ("it needs to be more
                // zoomed in otherwise the map is overwhelming and cluttered": 120 m to a pixel, it was 400), ON A NARROW
                // START ("just Dorestead and the closest port"): home and Traiectum are known, and beyond what she sees
                // from her own quay the chart is the fog's dark.
                Assert.AreEqual(120.0, map.chartScale, 0.01, "the chart's own scale");
                Assert.AreEqual(("dorestad", "traiectum"), KapmanGame.StartKnows, "the first morning's chart is not opened round home and the port nearest it");
                float Seen(string port)
                {
                    var p = GameData.Port(port);
                    var (wx, wz) = WorldFrame.ToWorld(p.lat, p.lon);
                    return Exploration.Seen(wx, wz);
                }
                Assert.AreEqual(1f, Seen("dorestad"), 1e-6f, "home is not known on the first morning");
                Assert.Greater(Seen("traiectum"), 0.9f, "the port nearest home is not known on the first morning");
                foreach (string far in new[] { "witla", "walichrum", "medemelacha", "daventre", "colonia" })
                    Assert.AreEqual(0f, Seen(far), 1e-6f, GameData.Port(far).name + " is known on the first morning");
                {
                    var px = play.ScreenPixels(out int width, out int height);
                    var dark = Exploration.Dark;
                    Vector2 byHome = Px(new WorldXZ(her.x + 9000.0, her.z + 9000.0)), east = Px(new WorldXZ(her.x + 58000.0, her.z - 4000.0));
                    Assert.IsTrue(hole.Contains(byHome) && hole.Contains(east), $"the test's two places are not on the chart: {byHome}, {east}");
                    var landByHome = MiddleOf(px, width, height, byHome);
                    var landEast = MiddleOf(px, width, height, east);
                    Debug.Log($"Map zoom: on the first morning the land 13 km from her quay is drawn {landByHome}, the land 58 km east of it {landEast}; the fog is {dark}");
                    Assert.Greater(Apart(landByHome, dark), 40, $"the land in sight of her own quay is as dark as the unknown: drawn {landByHome}");
                    Assert.LessOrEqual(Apart(landEast, dark), 12, $"58 km east of home the chart shows on the first morning: drawn {landEast}, the fog is {dark}");
                }

                // A PLACE IS ITS DOT: no name is up until the pointer rests on a dot
                Assert.AreEqual(7f, map.nameLingerSeconds, 1e-4f, "a name should stay seven seconds after the pointer has left its dot");
                Assert.AreEqual(0, NamesUp(play), "a name stands on the chart with no pointer on its dot");
                var homeMark = play.Screen.Q<VisualElement>("map-port-dorestad");
                var homeName = play.Screen.Q<VisualElement>("map-port-name-dorestad");
                Assert.IsNotNull(homeMark, "Dorestat has no dot on the chart");
                Assert.IsNotNull(homeName, "Dorestat's dot has no name to bring up");
                Assert.IsTrue(play.Reachable(homeMark), "the pointer cannot rest on Dorestat's dot");
                Assert.Less((homeMark.worldBound.center - Px(her)).magnitude, 30f, "Dorestat's dot is not where she lies");
                yield return play.Shot("zoom_0_the_first_morning");
                // (the test does not wait seven seconds: the same rule with a shorter stay)
                map.nameLingerSeconds = 1.5f;
                var openChart = middle + new Vector2(260f, 190f);
                play.MoveTo(homeMark.worldBound.center);
                yield return ChartPlay.Frames(5);
                Assert.IsTrue(ChartPlay.Shown(homeName), "the pointer on Dorestat's dot did not bring its name up");
                Assert.AreEqual(1f, map.NameShown("dorestad"), 0.01f, "under the pointer the name is not whole");
                Assert.AreEqual(GameData.Port("dorestad").name, homeName.Q<Label>(className: "map-port-name").text);
                Assert.Greater(homeName.worldBound.xMin, homeMark.worldBound.center.x, "the name does not stand right of its dot");
                Assert.Less(homeName.worldBound.xMin - homeMark.worldBound.center.x, 24f, "the name stands far from its dot");
                Assert.AreEqual(1, NamesUp(play), "more than the one name under the pointer is up");
                yield return play.Shot("zoom_0_a_name_under_the_pointer");
                // the pointer leaves: the name stays its time, then fades, and the place is its dot again
                play.MoveTo(openChart);
                float leftAt = Time.realtimeSinceStartup;
                yield return ChartPlay.Frames(3);
                Assert.AreEqual(1f, map.NameShown("dorestad"), 0.01f, "the name went the moment the pointer left its dot");
                while (Time.realtimeSinceStartup - leftAt < map.nameLingerSeconds - 0.5f)
                    yield return null;
                Assert.AreEqual(1f, map.NameShown("dorestad"), 0.01f, "the name did not stay its time after the pointer had left");
                bool fading = false;
                while (Time.realtimeSinceStartup - leftAt < map.nameLingerSeconds + map.nameFadeSeconds + 0.7f)
                {
                    float shown = map.NameShown("dorestad");
                    fading |= shown > 0.03f && shown < 0.97f;
                    yield return null;
                }
                Assert.IsTrue(fading, "the name was cut off: it should fade");
                Assert.AreEqual(0f, map.NameShown("dorestad"), "the name is still up after its time and its fade");
                Assert.IsFalse(ChartPlay.Shown(homeName), "after its time the place is not just its dot again");
                Assert.AreEqual(0, NamesUp(play));
                // a port only heard of is a hollow dot in the dark, and its name says so
                VisualElement heardMark = null;
                string heardId = null;
                foreach (var def in GameData.Ports)
                {
                    var m = play.Screen.Q<VisualElement>("map-port-" + def.id);
                    if (heardMark == null && m != null && m.ClassListContains("heard") && ChartPlay.Shown(m) && hole.Contains(m.worldBound.center)
                        && Seen(def.id) == 0f && play.Reachable(m))
                    {
                        heardMark = m;
                        heardId = def.id;
                    }
                }
                Assert.IsNotNull(heardMark, "no port she has only heard of has a dot on the first morning's chart");
                {
                    var px = play.ScreenPixels(out int width, out int height);
                    var beside = MiddleOf(px, width, height, heardMark.worldBound.center + new Vector2(0f, 26f));
                    Assert.LessOrEqual(Apart(beside, Exploration.Dark), 12, $"round the dot of {heardId}, only heard of, the chart shows: drawn {beside}");
                }
                play.MoveTo(heardMark.worldBound.center);
                yield return ChartPlay.Frames(5);
                var heardName = play.Screen.Q<VisualElement>("map-port-name-" + heardId);
                Assert.IsTrue(heardName != null && ChartPlay.Shown(heardName), "the pointer on the dot of a port only heard of did not bring its name up");
                Assert.AreEqual(GameData.Port(heardId).name, heardName.Q<Label>(className: "map-port-name").text);
                Assert.AreEqual(MapView.HeardOfWords, heardName.Q<Label>(className: "map-port-note").text, "a port only heard of does not say so under its name");
                // a water she knows from the start is a mark on the chart, named under the pointer as a port is
                var rhine = play.Screen.Q<VisualElement>("map-find-rhenus");
                Assert.IsNotNull(rhine, "the Rhine at her own quay has no mark on the chart");
                Assert.IsTrue(Discoveries.IsFound("rhenus") && Discoveries.DayFound("rhenus") < 0.0, "her own river should be known to her from the start");
                Assert.IsTrue(play.Reachable(rhine), "the pointer cannot rest on the Rhine's mark");
                play.MoveTo(rhine.worldBound.center);
                yield return ChartPlay.Frames(5);
                var rhineName = play.Screen.Q<VisualElement>("map-find-name-rhenus");
                Assert.IsTrue(rhineName != null && ChartPlay.Shown(rhineName), "the pointer on the Rhine's mark did not bring its name up");
                Assert.AreEqual(Discoveries.Book.Get("rhenus").name, rhineName.Q<Label>(className: "map-port-name").text);
                Assert.AreEqual("river", rhineName.Q<Label>(className: "map-port-note").text, "a water does not say what it is under its name");
                Assert.AreEqual(1f, map.NameShown(heardId), 0.01f, "the name the pointer left a moment ago should still be up");
                yield return play.Shot("zoom_0_a_water_and_a_port_heard_of");
                // near the chart's edge the name stands on the other side of its dot, where it can be read
                play.MoveTo(openChart);
                yield return play.Drag(new Vector2(hole.xMin + 150f, hole.yMax - 120f), new Vector2(hole.xMin + 150f + (hole.xMax - 34f - homeMark.worldBound.center.x), hole.yMax - 120f));
                yield return ChartPlay.Frames(3);
                Assert.Greater(homeMark.worldBound.center.x, hole.xMax - 60f, "the drag did not bring Dorestat's dot to the chart's right edge");
                play.MoveTo(homeMark.worldBound.center);
                yield return ChartPlay.Frames(6);
                Assert.AreEqual(1f, map.NameShown("dorestad"), 0.01f, "at the chart's edge the name did not come up");
                Assert.Less(homeName.worldBound.xMax, homeMark.worldBound.center.x, "at the chart's right edge the name does not stand left of its dot");
                Assert.Greater(homeName.worldBound.xMin, hole.xMin, "the name runs off the chart");
                yield return play.Shot("zoom_0_a_name_at_the_edge");
                play.MoveTo(new Vector2(middle.x, 20f));                    // (off the chart: on the frame's top bar)
                yield return play.Press(Key.F);
                yield return ChartPlay.Settle(2.6f);                        // (she is in the middle again, and the names have gone)
                Assert.IsTrue(map.Following, "[F] did not bring the chart back to her");
                Assert.AreEqual(0, NamesUp(play), "names are still up with the pointer off the chart and their time over");

                // ---- 1. THE FOG LIES ON THE WORLD AT EVERY SCALE. Two squares of its own picture are painted red and green
                // (south-east of her, and out over the sea to the north-west: where no port's dot or name lies over
                // them), and looked for on the screen itself.
                var ext = Exploration.Extent;
                var fog = Exploration.Texture;
                int c0 = (int)System.Math.Floor((her.x - ext.west) / Exploration.CellSize), r0 = (int)System.Math.Floor((ext.north - her.z) / Exploration.CellSize);
                int c1 = c0 - 14, r1 = r0 - 6;
                c0 += 3;
                r0 += 6;
                void Paint(int c, int r, Color32 colour)
                {
                    var block = new Color32[9];
                    for (int i = 0; i < 9; i++)
                        block[i] = colour;
                    fog.SetPixels32(c - 1, fog.height - 1 - (r + 1), 3, 3, block);    // (the picture's rows run south first)
                }
                Paint(c0, r0, new Color32(255, 0, 0, 255));
                Paint(c1, r1, new Color32(0, 255, 0, 255));
                fog.Apply(false);
                var red = new WorldXZ(ext.west + (c0 + 0.5) * Exploration.CellSize, ext.north - (r0 + 0.5) * Exploration.CellSize);
                var green = new WorldXZ(ext.west + (c1 + 0.5) * Exploration.CellSize, ext.north - (r1 + 0.5) * Exploration.CellSize);
                var notes = new List<string>();
                int scales = 0;
                bool marked = false;
                // (out from the chart's own scale, where the squares are off the chart, to six times as far, and half
                // way back)
                foreach (int notches in new[] { 2, 2, 2, 2, -5 })
                {
                    yield return Wheel(play, middle, notches, 1.2f);
                    yield return null;
                    var px = play.ScreenPixels(out int width, out int height);
                    float square = (float)(Exploration.CellSize * 3.0 / map.Scale);
                    foreach (var (name, at, is_) in new (string, WorldXZ, System.Func<Color32, bool>)[]
                             {
                                 ("the red square", red, c => c.r > 200 && c.g < 70 && c.b < 70),
                                 ("the green square", green, c => c.g > 200 && c.r < 70 && c.b < 70),
                             })
                    {
                        Vector2 want = Px(at);
                        bool inView = want.x - square > hole.xMin && want.x + square < hole.xMax && want.y - square > hole.yMin && want.y + square < hole.yMax;
                        if (!inView)
                            continue;
                        Assert.IsTrue(Find(px, width, height, hole, is_, out var seen, out var size),
                                      $"at {map.Scale:0} m a pixel {name} of the fog is not on the screen; it should stand at {want}");
                        notes.Add($"{map.Scale:0} m/px: {name} at {seen}, {size.x:0} px wide; it should be at {want}, {square:0} px");
                        float slack = Mathf.Max(4f, square * 0.12f);
                        Assert.Less(Mathf.Abs(seen.x - want.x), slack, $"at {map.Scale:0} m a pixel {name} of the fog is drawn at {seen}, not at {want}: the fog does not lie on the world");
                        Assert.Less(Mathf.Abs(seen.y - want.y), slack, $"at {map.Scale:0} m a pixel {name} of the fog is drawn at {seen}, not at {want}: the fog does not lie on the world");
                        Assert.Greater(size.x, square * 0.6f, $"at {map.Scale:0} m a pixel {name} of the fog is {size.x:0} px wide and should be about {square:0}: the fog is not at the chart's scale");
                        Assert.Less(size.x, square * 1.15f, $"at {map.Scale:0} m a pixel {name} of the fog is {size.x:0} px wide and should be about {square:0}: the fog is not at the chart's scale");
                        scales++;
                    }
                    if (!marked && scales >= 2)
                    {
                        marked = true;
                        yield return play.Shot("zoom_1_the_fog_marked", 0.1f);
                    }
                }
                Assert.GreaterOrEqual(scales, 7, "the fog's squares were looked for at too few scales: " + string.Join(" | ", notes));
                Debug.Log("Map zoom: the fog on the world: " + string.Join(" | ", notes));
                FogAgain();                                                 // (the fog as the game has it again)
                yield return ChartPlay.Frames(3);
                {
                    // (and the marks are gone from the screen)
                    var px = play.ScreenPixels(out int width, out int height);
                    Assert.IsFalse(Find(px, width, height, hole, c => c.r > 200 && c.g < 70 && c.b < 70, out _, out _), "the test's red square is still in the fog");
                }
                yield return play.Press(Key.F);
                yield return Wheel(play, middle, 0);
                // back to the chart's own scale
                for (int i = 0; i < 40 && System.Math.Abs(map.ScaleWanted - map.chartScale) > map.chartScale * 0.05; i++)
                    yield return Wheel(play, middle, map.ScaleWanted > map.chartScale ? -1 : 1, 0.2f);
                yield return ChartPlay.Settle(1.0f);

                // ---- 2. FOLLOWED, SHE KEEPS HER PLACE WHILE THE WHEEL TURNS, and the view closes on its scale over some
                // frames (it jumped a notch at once, lurched off her and slid back)
                Assert.IsTrue(map.Following);
                Vector2 herAt = ship.worldBound.center;
                double scaleWas = map.Scale;
                play.WheelAt(middle, 3f);
                play.WheelAt(middle, 3f);
                play.WheelAt(middle, 3f);
                yield return null;
                Assert.Greater(map.ScaleWanted, scaleWas * 1.5, "three notches out did not ask for a wider scale");
                Assert.Greater(map.Scale, scaleWas * 1.001, "the view did not begin to close on the scale asked for");
                // (a frame that took a third of a second of the clock on the wall would have closed on it all the same)
                if (Time.unscaledDeltaTime < 0.15f)
                    Assert.Less(map.Scale, map.ScaleWanted * 0.995, "the view jumped to the scale asked for in one frame: it should close on it");
                float worst = 0f;
                bool monotone = true;
                double last = map.Scale;
                int frames = 0;
                // (the view closes on its scale by the clock on the wall: a second and a half of it)
                for (float t0 = Time.realtimeSinceStartup; Time.realtimeSinceStartup - t0 < 1.5f; frames++)
                {
                    yield return null;
                    worst = Mathf.Max(worst, (ship.worldBound.center - herAt).magnitude);
                    monotone &= map.Scale >= last - 1e-6;
                    last = map.Scale;
                }
                Assert.Greater(frames, 5, "too few frames were looked at");
                Assert.AreEqual(map.ScaleWanted, map.Scale, map.ScaleWanted * 0.002, "a second and a half after the wheel the view is still not at its scale");
                Assert.IsTrue(monotone, "zooming out, the scale went back and forth");
                Assert.Less(worst, 2.5f, $"followed, she moved {worst:0.0} px on the screen while the wheel turned: she should keep her place");
                Assert.IsTrue(map.Following, "the wheel stopped the chart following her");
                yield return play.Shot("zoom_2_followed_and_out");

                // ---- 3. NOT FOLLOWED, THE PLACE UNDER THE POINTER STAYS UNDER IT. The chart is dragged (she is no longer
                // followed); then the wheel is turned over a port's dot, and the dot stays under the pointer.
                yield return play.Drag(middle + new Vector2(120f, 40f), middle + new Vector2(-60f, -20f));
                Assert.IsFalse(map.Following, "a drag did not take the chart off her");
                yield return ChartPlay.Settle(0.4f);
                VisualElement dot = null;
                play.Screen.Query<VisualElement>(className: "map-port").ForEach(m =>
                {
                    var d = m.Q<VisualElement>(className: "map-port-dot");
                    if (dot == null && ChartPlay.Shown(m) && d != null && hole.Contains(d.worldBound.center)
                        && (d.worldBound.center - middle).magnitude > 150f && (d.worldBound.center - middle).magnitude < 420f)
                        dot = d;
                });
                Assert.IsNotNull(dot, "no port's dot stands off the middle of the chart to turn the wheel over");
                Vector2 pointer = dot.worldBound.center;
                worst = 0f;
                play.WheelAt(pointer, -3f);
                play.WheelAt(pointer, -3f);
                for (float t0 = Time.realtimeSinceStartup; Time.realtimeSinceStartup - t0 < 1.2f;)
                {
                    yield return null;
                    worst = Mathf.Max(worst, (dot.worldBound.center - pointer).magnitude);
                }
                Assert.AreEqual(map.ScaleWanted, map.Scale, map.ScaleWanted * 0.002, "the view did not close on the scale the wheel asked for over the port");
                Assert.Less(worst, 2.5f, $"not followed, the place under the pointer moved {worst:0.0} px away from it while the wheel turned");
                Assert.IsFalse(map.Following);

                // ---- 4. THE VIEW NEVER LEAVES THE PAINTED CHART: by the wheel out to the widest, nor by the hand
                yield return Wheel(play, middle, 30, 1.6f);
                Assert.AreEqual(map.Widest, map.Scale, map.Widest * 0.002, "thirty notches out the chart is not at its widest");
                Assert.Less(map.Widest, map.maxScale * 0.6, "the widest is not set by the painted chart");
                void Inside(string when)
                {
                    var s = map.Showing;
                    double slack = map.Scale * 1.5;
                    Assert.IsTrue(s.west >= artWest - slack && s.east <= artEast + slack && s.north <= artNorth + slack && s.south >= artSouth - slack,
                                  $"{when} the chart shows beyond its painted picture: west {s.west:0} to east {s.east:0}, north {s.north:0} to south {s.south:0}; " +
                                  $"the picture is {artWest:0} to {artEast:0}, {artNorth:0} to {artSouth:0}");
                }
                Inside("At its widest");
                yield return play.Shot("zoom_3_the_widest");
                // THE UNKNOWN IS HIDDEN (the user, 9 Oct 2026: "Shouldn't it be you can't see anything you haven't been?").
                // Far from home the sea and the land are the same dark, the fog's own, with nothing of the chart showing
                // through.
                {
                    var px = play.ScreenPixels(out int width, out int height);
                    Vector2 At(double lat, double lon, string what)
                    {
                        var (wx, wz) = WorldFrame.ToWorld(lat, lon);
                        Vector2 at = Px(new WorldXZ(wx, wz));
                        Assert.IsTrue(hole.Contains(at), $"{what} is not on the chart at its widest: {at}");
                        return at;
                    }
                    var dark = Exploration.Dark;
                    var sea = MiddleOf(px, width, height, At(57.0, 3.5, "the middle of the North Sea"));
                    var land = MiddleOf(px, width, height, At(54.3, -2.2, "the hills of northern England"));
                    var coast = MiddleOf(px, width, height, At(53.0, 4.0, "the sea off the Frisian coast"));
                    Debug.Log($"Map zoom: the unknown sea is drawn {sea}, the unknown land {land}, the sea off Frisia {coast}; the fog's own colour is {dark}");
                    Assert.LessOrEqual(Apart(sea, dark), 12, $"the sea she has never seen shows through the fog: drawn {sea}, the fog is {dark}");
                    Assert.LessOrEqual(Apart(land, dark), 12, $"the land she has never seen shows through the fog: drawn {land}, the fog is {dark}");
                    Assert.LessOrEqual(Apart(coast, dark), 12, $"the sea off Frisia, 150 km from home, shows on the first morning: drawn {coast}, the fog is {dark}");
                }
                // far out too a place is only its dot: the ports she has heard of are dots in the dark, and no name is up
                play.MoveTo(new Vector2(middle.x, 20f));
                yield return ChartPlay.Settle(map.nameLingerSeconds + map.nameFadeSeconds + 0.4f);
                Assert.AreEqual(0, NamesUp(play), "at its widest names stand on the chart with no pointer on a dot");
                int dots = 0;
                play.Screen.Query<VisualElement>(className: "map-port").ForEach(m =>
                {
                    if (ChartPlay.Shown(m) && hole.Contains(m.worldBound.center))
                        dots++;
                });
                Assert.Greater(dots, 5, "at its widest the chart shows next to no port");
                foreach (var to in new[] { new Vector2(700f, 0f), new Vector2(-700f, 0f), new Vector2(0f, 500f), new Vector2(0f, -500f) })
                {
                    for (int i = 0; i < 3; i++)
                        yield return play.Drag(middle - to * 0.45f, middle + to * 0.45f);
                    yield return ChartPlay.Frames(3);
                    Inside("Dragged " + to + ",");
                }
                yield return play.Shot("zoom_4_dragged_to_its_edge");

                // ---- 5. BACK TO HER, IN TO THE CLOSEST, AND ON PAST IT: the ship's own view is asked for, once
                yield return play.Press(Key.F);
                yield return ChartPlay.Settle(1.5f);
                Assert.IsTrue(map.Following, "[F] did not bring the chart back to her");
                map.ZoomedPast += onPast;
                for (int i = 0; i < 60 && map.ScaleWanted > map.Closest * 1.001; i++)
                    yield return Wheel(play, middle, -1, 0.05f);
                yield return ChartPlay.Settle(1.2f);
                Assert.AreEqual(map.Closest, map.Scale, map.Closest * 0.01, "the wheel did not bring the chart to its closest");
                Assert.Less((ship.worldBound.center - middle).magnitude, 8f, "at its closest she is not in the middle");
                Assert.AreEqual(0, asked, "the ship's view was asked for before the chart was at its closest and scrolled past");
                yield return play.Shot("zoom_5_the_closest");
                play.WheelAt(middle, -3f);
                yield return null;
                Assert.AreEqual(0, asked, "one notch past the closest already took the chart away");
                play.WheelAt(middle, -3f);
                yield return null;
                Assert.AreEqual(1, asked, "two notches past the closest should ask for the ship's own view");
                yield return ChartPlay.Settle(0.8f);
                Assert.AreEqual(0, play.faults.Count, "faults a reader would see on screen:\n" + string.Join("\n", play.faults));
            }
            finally
            {
                if (MapView.Current != null)
                {
                    MapView.Current.ZoomedPast -= onPast;
                    MapView.Current.nameLingerSeconds = 7f;
                }
                var hook = Kapman.Game.ShipView.ShipStageHook.Current;
                if (hook != null)
                    hook.Swapped = false;
                if (Exploration.Ready)
                    FogAgain();
                play.End();
                FrameRoot.FreshLayout = false;
                ClockSettings.AsNew = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
