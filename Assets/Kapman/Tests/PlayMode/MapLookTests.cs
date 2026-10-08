using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.Levels;
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
    /// A LOOK AT THE CHART'S ZOOM AND ITS FOG (9 Oct 2026; the user: "Can you fix the map zoom and the fog of war").
    /// The chart is zoomed by the wheel from its widest to its closest and on into the ship's own view, a picture at
    /// each step with the scale it stands at; then she sails, and the fog is pictured as it lifts. The pictures go to
    /// Captures/maplook with look.txt beside them. Not part of the suites: run it by its name, to see. What it showed
    /// is held by MapZoomTests.
    /// </summary>
    public class MapLookTests
    {
        static double Mpp => MapView.Current != null ? MapView.Current.Scale : 0.0;
        static bool Following => MapView.Current != null && MapView.Current.Following;

        static Button Verb(ChartPlay play, string name) =>
            play.Buttons(name, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-verb"));

        static Button Small(ChartPlay play, string words) =>
            play.Buttons(words, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-k"));

        static IEnumerator AnswerAll(ChartPlay play)
        {
            for (int i = 0; i < 12 && play.FrontFloat() != null; i++)
            {
                var front = play.FrontFloat();
                for (int k = 0; k < 30 && play.Frame.FrontReveal != null && !play.Frame.FrontReveal.Done; k++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(4);
                }
                var pick = play.Choices(front).FirstOrDefault(c => c.can);
                if (pick.button != null)
                    yield return play.Click(pick.button, pick.words);
                else
                {
                    var cross = front.Q<Button>(className: "frame-float-shut");
                    if (cross == null)
                        yield break;
                    yield return play.Click(cross, "the cross");
                }
                yield return ChartPlay.Frames(4);
            }
        }

        [UnityTest, Timeout(1800000)]
        public IEnumerator TheZoomAndTheFogAsTheyAre()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            ClockDrops.Quiet = true;
            ClockSettings.AsNew = true;
            string saves = Path.Combine(Application.temporaryCachePath, "kapman_maplook_saves");
            Directory.CreateDirectory(saves);
            foreach (string old in Directory.GetFiles(saves))
                File.Delete(old);
            SaveGame.FolderOverride = saves;
            var load = SceneManager.LoadSceneAsync("L_Chart", LoadSceneMode.Single);
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
            var play = ChartPlay.Begin(true, "maplook");
            var said = new StringBuilder();
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "maplook");
            try
            {
                yield return ChartPlay.Frames(120);
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                from = Time.realtimeSinceStartup;
                while (FrameGame.PortHere == null && Time.realtimeSinceStartup - from < 15f)
                    yield return null;
                yield return ChartPlay.Frames(30);
                var game = KapmanGame.Current;
                var map = MapView.Current;
                // (the chart in the middle, not the port's picture)
                var view = PortView.Current;
                if (view != null && view.Shown)
                {
                    yield return play.Click(view.Flip, "Chart");
                    yield return ChartPlay.Frames(6);
                }
                var middle = new Vector2(1180f, 430f);
                void Note(string line)
                {
                    said.AppendLine(line);
                    Debug.Log("Map look: " + line);
                }

                Note($"at the start: {Mpp:0.#} m a pixel, following {Following}; chartScale {map.chartScale}, min {map.minScale}, max {map.maxScale}");
                yield return play.Shot("00_at_the_start");

                // ---- out, a notch at a time, to the widest
                int notch = 0;
                double last = Mpp;
                for (int i = 0; i < 60; i++)
                {
                    play.WheelAt(middle, 3f);
                    notch++;
                    yield return ChartPlay.Settle(0.5f);
                    if (System.Math.Abs(Mpp - last) < 0.001 * last)
                    {
                        Note($"out: the widest is {Mpp:0.#} m a pixel, reached after {notch - 1} notches from the start");
                        break;
                    }
                    last = Mpp;
                    if (notch % 3 == 0)
                    {
                        Note($"out, notch {notch}: {Mpp:0.#} m a pixel, following {Following}");
                        yield return play.Shot($"01_out_{notch:00}_{Mpp:0}_m_a_px", 0.8f);
                    }
                }
                yield return ChartPlay.Settle(2.5f);
                yield return play.Shot("02_the_widest", 0.8f);
                File.WriteAllBytes(Path.Combine(dir, "fog_at_the_start.png"), Exploration.Texture.EncodeToPNG());
                var ext = Exploration.Extent;
                Note($"the fog's picture: {Exploration.Texture.width} x {Exploration.Texture.height} squares of {Exploration.CellSize / 1000.0:0} km; west {ext.west:0}, north {ext.north:0}, east {ext.east:0}, south {ext.south:0}");

                // ---- in, a notch at a time, to the closest
                notch = 0;
                last = Mpp;
                for (int i = 0; i < 80; i++)
                {
                    play.WheelAt(middle, -3f);
                    notch++;
                    yield return ChartPlay.Settle(0.4f);
                    if (System.Math.Abs(Mpp - last) < 0.001 * last)
                    {
                        Note($"in: the closest is {Mpp:0.##} m a pixel, reached after {notch - 1} notches from the widest; following {Following}");
                        break;
                    }
                    last = Mpp;
                    if (notch % 5 == 0)
                    {
                        Note($"in, notch {notch}: {Mpp:0.#} m a pixel, following {Following}");
                        yield return play.Shot($"03_in_{notch:00}_{Mpp:0}_m_a_px", 0.8f);
                    }
                }
                yield return ChartPlay.Settle(2.0f);
                yield return play.Shot("04_the_closest", 0.8f);

                // ---- [F]: back to her
                yield return play.Press(Key.F);
                yield return ChartPlay.Settle(2.5f);
                Note($"after [F]: {Mpp:0.#} m a pixel, following {Following}");
                yield return play.Shot("05_after_f", 0.6f);

                // ---- in on her to the closest, and on past it: her own view
                for (int i = 0; i < 80; i++)
                {
                    double was = Mpp;
                    play.WheelAt(middle, -3f);
                    yield return ChartPlay.Settle(0.25f);
                    if (System.Math.Abs(Mpp - was) < 0.001 * was)
                        break;
                }
                yield return ChartPlay.Settle(1.5f);
                Note($"in on her: {Mpp:0.##} m a pixel, following {Following}");
                yield return play.Shot("06_in_on_her", 0.6f);
                var hook = Kapman.Game.ShipView.ShipStageHook.Current;
                for (int i = 0; i < 4; i++)
                {
                    play.WheelAt(middle, -3f);
                    yield return ChartPlay.Settle(0.5f);
                }
                yield return ChartPlay.Settle(2.5f);
                Note($"scrolled on in past the closest: the ship's view in the middle: {(hook != null ? hook.Swapped.ToString() : "no hook")}; {Mpp:0.##} m a pixel");
                yield return play.Shot("07_past_the_closest", 0.8f);
                for (int i = 0; i < 4; i++)
                {
                    play.WheelAt(middle, 3f);
                    yield return ChartPlay.Settle(0.5f);
                }
                yield return ChartPlay.Settle(2.0f);
                Note($"scrolled out again: the ship's view in the middle: {(hook != null ? hook.Swapped.ToString() : "no hook")}; {Mpp:0.##} m a pixel");
                yield return play.Shot("08_scrolled_out_again", 0.8f);
                if (hook != null && hook.Swapped)
                {
                    hook.Swapped = false;
                    yield return ChartPlay.Settle(1.5f);
                }

                // ---- she sails: the fog as it lifts
                yield return play.Press(Key.F);
                yield return ChartPlay.Settle(2.0f);
                Assert.IsTrue(Course.SetTo("ripa", ChartWorld.ShipWorld(), Port.LyingId()), "no course to Ripa");
                yield return ChartPlay.Frames(5);
                Note($"a course set: {Mpp:0.#} m a pixel, following {Following}");
                yield return play.Shot("10_a_course_set", 1.5f);
                yield return play.Click(Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return play.Click(Small(play, "5"), "speed 5");
                double day0 = game.Day;
                int shots = 0;
                float wall = Time.realtimeSinceStartup;
                while (game.Day - day0 < 3.2 && Time.realtimeSinceStartup - wall < 240f)
                {
                    if (play.FrontFloat() != null)
                    {
                        yield return AnswerAll(play);
                        if (GameSpeed.Level < 4 && play.FrontFloat() == null)
                            yield return play.Click(Small(play, "5"), "speed 5");
                    }
                    if (game.Day - day0 > 0.5 + shots * 0.9 && shots < 3)
                    {
                        shots++;
                        Note($"at sea, day {game.Day - day0:0.0}: {Mpp:0.#} m a pixel, following {Following}");
                        yield return play.Shot($"11_at_sea_{shots}", 0.4f);
                    }
                    yield return null;
                }
                yield return play.Click(Small(play, "1"), "speed 1");
                yield return AnswerAll(play);
                // out wide, to see her whole track through the fog
                for (int i = 0; i < 9; i++)
                {
                    play.WheelAt(middle, 3f);
                    yield return ChartPlay.Settle(0.3f);
                }
                yield return ChartPlay.Settle(2.5f);
                Note($"her track, from far out: {Mpp:0.#} m a pixel, following {Following}");
                yield return play.Shot("12_her_track_from_far_out", 0.8f);
                File.WriteAllBytes(Path.Combine(dir, "fog_after_three_days.png"), Exploration.Texture.EncodeToPNG());
                for (int i = 0; i < 9; i++)
                {
                    play.WheelAt(middle, 3f);
                    yield return ChartPlay.Settle(0.3f);
                }
                yield return ChartPlay.Settle(2.5f);
                Note($"and from the widest: {Mpp:0.#} m a pixel");
                yield return play.Shot("13_her_track_from_the_widest", 0.8f);
            }
            finally
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "look.txt"), said.ToString());
                play.End();
                FrameRoot.FreshLayout = false;
                ClockSettings.AsNew = false;
                GameSpeed.SetLevel(0);
            }
        }

        /// <summary>
        /// THE FIRST MORNING AND WHAT SHE FINDS (9 Oct 2026, later; the user: "just simply can't see the name of
        /// somewhere, narrow the starting fog area, just Dorestead and the closest port, then add waters to discover and
        /// land marks", "hide the name ... unless you're hovering over the dot", "it needs to be more zoomed in"). A
        /// look, with pictures: the chart as a new game opens it; a name under the pointer; she sails for Ripa, and each
        /// water or landmark she finds on the way with what the log says of it; Journey's list of them; her track
        /// through the dark from farther out. Pictures 20_*.png to 24_*.png in Captures/maplook, and finds.txt. Not part
        /// of the suites: run it by its name, to see. What it shows is held by MapZoomTests and DiscoveriesTests.
        /// </summary>
        [UnityTest, Timeout(1800000)]
        public IEnumerator TheFirstMorningAndWhatSheFinds()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = true;
            ClockDrops.Quiet = true;
            ClockSettings.AsNew = true;
            string saves = Path.Combine(Application.temporaryCachePath, "kapman_maplook_saves");
            Directory.CreateDirectory(saves);
            foreach (string old in Directory.GetFiles(saves))
                File.Delete(old);
            SaveGame.FolderOverride = saves;
            var load = SceneManager.LoadSceneAsync("L_Chart", LoadSceneMode.Single);
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
            var play = ChartPlay.Begin(false, "maplook");
            var said = new StringBuilder();
            string dir = Path.Combine(Application.dataPath, "..", "Captures", "maplook");
            try
            {
                yield return ChartPlay.Frames(120);
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                from = Time.realtimeSinceStartup;
                while (FrameGame.PortHere == null && Time.realtimeSinceStartup - from < 15f)
                    yield return null;
                yield return ChartPlay.Frames(30);
                var game = KapmanGame.Current;
                var map = MapView.Current;
                var view = PortView.Current;
                if (view != null && view.Shown)
                {
                    yield return play.Click(view.Flip, "Chart");
                    yield return ChartPlay.Frames(6);
                }
                var middle = new Vector2(1180f, 430f);
                var offChart = new Vector2(700f, 780f);                     // (a corner of the chart with no mark in it, and no hover card)
                void Note(string line)
                {
                    said.AppendLine(line);
                    Debug.Log("Map look: " + line);
                }
                yield return ChartPlay.Settle(1.5f);
                var heard = GameData.Ports.Where(p => Known.IsHeardOf(p)).Select(p => p.name).ToList();
                Note($"the first morning: {Mpp:0.#} m a pixel; the chart is opened round {KapmanGame.StartKnows.home} and {KapmanGame.StartKnows.near}; " +
                     $"{Discoveries.Count} of {Discoveries.Total(game.Year)} waters and landmarks known ({string.Join(", ", Discoveries.FoundIds)}); " +
                     $"{heard.Count} ports heard of ({string.Join(", ", heard)})");
                yield return play.Shot("20_the_first_morning", 0.5f);
                var homeMark = play.Screen.Q<VisualElement>("map-port-dorestad");
                Assert.IsNotNull(homeMark, "Dorestat has no dot");
                play.MoveTo(homeMark.worldBound.center);
                yield return ChartPlay.Frames(8);
                yield return play.Shot("21_a_name_under_the_pointer", 0.2f);
                play.MoveTo(offChart);

                // ---- she sails for Ripa: down the Lek to the sea, and north along the islands
                Assert.IsTrue(Course.SetTo("ripa", ChartWorld.ShipWorld(), Port.LyingId()), "no course to Ripa");
                yield return ChartPlay.Frames(5);
                yield return play.Click(Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return play.Click(Small(play, "5"), "speed 5");
                int had = Discoveries.Count, shots = 0;
                double day0 = game.Day;
                float wall = Time.realtimeSinceStartup;
                while (game.Day - day0 < 4.5 && Time.realtimeSinceStartup - wall < 420f && Port.LyingId() != "ripa")
                {
                    if (play.FrontFloat() != null)
                    {
                        yield return AnswerAll(play);
                        if (GameSpeed.Level < 4 && play.FrontFloat() == null)
                            yield return play.Click(Small(play, "5"), "speed 5");
                    }
                    if (Discoveries.Count > had)
                    {
                        string id = Discoveries.FoundIds[had];
                        had++;
                        var thing = Discoveries.Book.Get(id);
                        var line = FrameLog.Lines.LastOrDefault(l => l.text.StartsWith("New to you: " + thing.name));
                        Note($"day {game.Day - day0:0.00}: found {id} ({thing.kindName}); the log: '{(line.text ?? "NOTHING")}'");
                        if (shots < 4)
                        {
                            shots++;
                            yield return play.Click(Small(play, "Pause"), "Pause");
                            yield return ChartPlay.Settle(0.6f);
                            var mark = play.Screen.Q<VisualElement>("map-find-" + id);
                            if (mark != null && ChartPlay.Shown(mark))
                            {
                                play.MoveTo(mark.worldBound.center);
                                yield return ChartPlay.Frames(8);
                            }
                            yield return play.Shot($"22_found_{shots}_{id}", 0.3f);
                            play.MoveTo(offChart);
                            if (play.FrontFloat() != null)
                                yield return AnswerAll(play);
                            yield return play.Click(Small(play, "5"), "speed 5");
                        }
                    }
                    yield return null;
                }
                Note($"after {game.Day - day0:0.0} days: {Discoveries.Count} of {Discoveries.Total(game.Year)} found: {string.Join(", ", Discoveries.FoundIds)}");
                yield return play.Click(Small(play, "Pause"), "Pause");
                yield return AnswerAll(play);

                // ---- Journey: what she has learned, the waters and landmarks among it
                yield return play.Click(Verb(play, "Journey"), "Journey");
                play.MoveTo(new Vector2(700f, 780f));                       // (off the button: its hover card would lie over the window)
                yield return ChartPlay.Settle(0.8f);
                yield return play.Shot("23_journey", 0.3f);

                // ---- from farther out: her track through the dark, and the marks along it
                for (int i = 0; i < 6; i++)
                {
                    play.WheelAt(middle, 3f);
                    yield return ChartPlay.Settle(0.3f);
                }
                yield return ChartPlay.Settle(2.0f);
                Note($"her track, from farther out: {Mpp:0.#} m a pixel");
                yield return play.Shot("24_her_track", 0.5f);
                // the pointer on one of the marks along it
                VisualElement along = null;
                foreach (string id in Discoveries.FoundIds)
                {
                    var m = play.Screen.Q<VisualElement>("map-find-" + id);
                    if (m != null && ChartPlay.Shown(m) && Discoveries.DayFound(id) >= 0.0 && play.Reachable(m))
                        along = m;
                }
                if (along != null)
                {
                    play.MoveTo(along.worldBound.center);
                    yield return ChartPlay.Frames(8);
                    yield return play.Shot("25_a_mark_along_her_track", 0.2f);
                }
            }
            finally
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "finds.txt"), said.ToString());
                play.End();
                FrameRoot.FreshLayout = false;
                ClockSettings.AsNew = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
