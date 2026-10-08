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
    ///   - no port's name lies over another, and the port she lies at keeps its name;
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
                foreach (int notches in new[] { 0, 2, 2, -7, -3 })
                {
                    if (notches != 0)
                        yield return Wheel(play, middle, notches, 1.2f);
                    else
                        yield return ChartPlay.Settle(0.5f);
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
                    if (notches == 2 && scales <= 4)
                        yield return play.Shot("zoom_1_the_fog_marked", 0.1f);
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
                // through; the water she knows, off her own coast, is not.
                {
                    var px = play.ScreenPixels(out int width, out int height);
                    Color32 Middle(double lat, double lon, string what)
                    {
                        var (wx, wz) = WorldFrame.ToWorld(lat, lon);
                        Vector2 at = Px(new WorldXZ(wx, wz));
                        Assert.IsTrue(hole.Contains(at), $"{what} is not on the chart at its widest: {at}");
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
                        return new Color32(r[r.Count / 2], g[g.Count / 2], b[b.Count / 2], 255);   // (the middle one: a speck or a mark's edge does not count)
                    }
                    int Far(Color32 a, Color32 b) => Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Max(Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b)));
                    var dark = Exploration.Dark;
                    var sea = Middle(57.0, 3.5, "the middle of the North Sea");
                    var land = Middle(54.3, -2.2, "the hills of northern England");
                    var home = Middle(53.0, 4.0, "the sea off her own coast");
                    Debug.Log($"Map zoom: the unknown sea is drawn {sea}, the unknown land {land}, the fog's own colour is {dark}; the sea she knows {home}");
                    Assert.LessOrEqual(Far(sea, dark), 12, $"the sea she has never seen shows through the fog: drawn {sea}, the fog is {dark}");
                    Assert.LessOrEqual(Far(land, dark), 12, $"the land she has never seen shows through the fog: drawn {land}, the fog is {dark}");
                    Assert.Greater(Far(home, dark), 40, $"the sea she knows, off her own coast, is as dark as the unknown: drawn {home}");
                }
                // (no name over another; the port she lies at has its name)
                var slips = new List<(string name, Rect at)>();
                play.Screen.Query<VisualElement>(className: "map-port").ForEach(m =>
                {
                    var label = m.Q<Label>(className: "map-port-name");
                    if (ChartPlay.Shown(m) && label != null && ChartPlay.Shown(label))
                        slips.Add((label.text, label.worldBound));
                });
                Assert.Greater(slips.Count, 5, "at its widest the chart names next to nothing");
                for (int i = 0; i < slips.Count; i++)
                    for (int k = i + 1; k < slips.Count; k++)
                        Assert.IsFalse(slips[i].at.Overlaps(slips[k].at), $"the names of {slips[i].name} and {slips[k].name} lie over one another");
                string here = FrameGame.PortHere.name;
                Assert.IsTrue(slips.Any(s => s.name == here), $"at its widest the chart does not name {here}, where she lies: {string.Join(", ", slips.Select(s => s.name))}");
                // a port without its name says it when the pointer rests on its dot
                int unnamed = 0;
                play.Screen.Query<VisualElement>(className: "map-port").ForEach(m =>
                {
                    var label = m.Q<Label>(className: "map-port-name");
                    if (ChartPlay.Shown(m) && label != null && !ChartPlay.Shown(label))
                    {
                        unnamed++;
                        Assert.AreEqual(label.text, m.tooltip, "a port left without its name does not say it under the pointer");
                    }
                });
                Assert.Greater(unnamed, 3, "at its widest every port still has its name: the heap was not thinned");
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
                    MapView.Current.ZoomedPast -= onPast;
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
